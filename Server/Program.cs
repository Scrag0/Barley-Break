using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

            //підключення нових TCP клієнтів та запуск методу ProcessClientAsync
            public void Work(TcpListener tcpListener)
            {
                try
                {
                    tcpListener.Start(); // запускає сервер
                    Console.WriteLine("The server is running. Waiting for connection...");

                    Dictionary<string, List<string>> GameHistory = new Dictionary<string, List<string>>();
                    List<TcpClient> clients = new List<TcpClient>();

                    while (true)
                    {
                        var tcpClient = tcpListener.AcceptTcpClient(); // приймаємо запит на підключення від TcpClient
                        clients.Add(tcpClient);
                        new Thread(async () => await ProcessClientAsync(tcpClient, GameHistory, clients)).Start(); // створюємо потік для отримання і відповіді на повідомленя від TcpClient
                    }
                }
                finally
                {
                    tcpListener.Stop();
                }
            }

            // метод на отримання та відповідь на повідомлення від клієна
            async Task ProcessClientAsync(TcpClient tcpClient, Dictionary<string, List<string>> GameHistory, List<TcpClient> clients)
            {
                var stream = tcpClient.GetStream();

                var response = new List<byte>(); // буфер для вхідних даних
                int bytesRead = 10;

                Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} connected");
                try
                {
                    while (true)
                    {

                        while ((bytesRead = stream.ReadByte()) != '\n') // зчитуємо дані до кінцевого символу
                        {
                            response.Add((byte)bytesRead); // додаємо в буфер
                        }

                        var Data = Encoding.UTF8.GetString(response.ToArray());

                        if (Data == "$Disconnect")
                        {
                            break;
                        }

                        foreach (var item in GameHistory)
                        {
                            //item.Value.OrderBy
                            if (item.Value.Count == 9)
                            {
                                item.Value.RemoveAt(0);
                            }
                        }

                        string startNumbers = Data.Split('$')[0];

                        if (!GameHistory.ContainsKey(startNumbers))
                        {
                            GameHistory.Add(startNumbers, new List<string>());
                        }
                        else
                        {

                        }    

                        GameHistory[startNumbers].Add(Data.Split('$')[1]);

                        Console.WriteLine($"New data: {Data.Split('$')[1]}");

                        string feedback = "";

                        foreach (var data in GameHistory)
                        {
                            feedback += data.Key + data.Value + "|";
                        }

                        feedback += "\n";
                        foreach (var client in clients)
                        {
                            await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(feedback));
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