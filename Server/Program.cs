using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //підключення до заданої в консолі Ip-адреси з портом
            var tcpListener = new TcpListener(IPAddress.Parse(args[0]), int.Parse(args[1]));

            Server server = new Server(tcpListener); // створення екземпляра сервера
        }

        public class Server
        {
            public Server(TcpListener tcpListener)
            {
                this.Work(tcpListener);
            }

            //підключення нових TCP клієнтів та запуск методу на отримання та відповіді на повідомлення від клієнта
            public void Work(TcpListener tcpListener)
            {
                try
                {
                    tcpListener.Start(); // запускає сервер
                    Console.WriteLine("The server is running. Waiting for connection...");

                    Dictionary<string, int> players = new Dictionary<string, int>();
                    List<TcpClient> clients = new List<TcpClient>();

                    while (true)
                    {
                        var tcpClient = tcpListener.AcceptTcpClient(); // приймаємо запит на підключення від TcpClient
                        clients.Add(tcpClient);
                        new Thread(async () => await ProcessClientAsync(tcpClient, players, clients)).Start(); // створюємо потік для отримання і відповіді на повідомленя від TcpClient
                    }
                }
                finally
                {
                    tcpListener.Stop();
                }
            }

            // метод на отримання та відповідь на повідомлення від клієна
            async Task ProcessClientAsync(TcpClient tcpClient, Dictionary<string, int> players, List<TcpClient> clients)
            {
                // словник з заготовленими відповідями на деякі повідомлення
                var stream = tcpClient.GetStream();
                
                var response = new List<byte>(); // буфер для вхідних даних
                int bytesRead = 10;

                try
                {
                    while (true)
                    {
                        while ((bytesRead = stream.ReadByte()) != '\n') // зчитуємо дані до кінцевого символу
                        {
                            response.Add((byte)bytesRead); // додаємо в буфер
                        }
                        var playerStringData = Encoding.UTF8.GetString(response.ToArray());

                        if (playerStringData == "$Disconnect")
                        {
                            break;
                        }

                        var playerData = playerStringData.Split();

                        string currentName = playerData[0];
                        int currentScore = Convert.ToInt32(playerData[1]);

                        // знаходимо користувача у словнику
                        if (!players.ContainsKey(currentName))
                        {
                            players.Add(currentName, currentScore);
                        }
                        else
                        {
                            var playerScore = players[currentName];

                            if (currentScore > playerScore)
                            {
                                Console.WriteLine($"New high score: {currentName} - {currentScore}");
                                players[currentName] = currentScore;
                            }
                        }

                        string feedback = "";

                        int i = 1;
                        foreach (var player in players.OrderByDescending(pair => pair.Value))
                        {
                            feedback += $"{i++}. {player.Key} - {player.Value}|";
                            if (i == 9)
                            {
                                break;
                            }
                        }

                        feedback += "\n";

                        foreach (var client in clients)
                        {
                            await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(feedback)); // віправляємо дані до кожного користувача
                        }

                        response.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} disconnected");
                    stream.Close();
                    clients.Remove(tcpClient);
                    tcpClient.Close();
                    tcpClient.Dispose();
                }
            }
        }
    }
}