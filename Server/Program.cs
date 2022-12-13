using System;
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

                    Dictionary<string, List<string[]>> GameHistory = new Dictionary<string, List<string[]>>();
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
            async Task ProcessClientAsync(TcpClient tcpClient, Dictionary<string, List<string[]>> GameHistory, List<TcpClient> clients)
            {
                var stream = tcpClient.GetStream();

                var response = new List<byte>(); // буфер для вхідних даних
                int bytesRead = 10;

                Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} connected");
                try
                {
                    SendAllLayouts(GameHistory, clients);

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
                        
                        if (GameHistory.Keys.Count == 11)
                        {
                            GameHistory.Remove(GameHistory.Keys.First());
                        }

                        foreach (var item in GameHistory.Keys)
                        {
                            GameHistory[item] = GameHistory[item].Where(x => x[2] != "").OrderBy(x => TimeOnly.Parse(x[2])).ThenBy(x => x[1]).ToList();
                            if (GameHistory[item].Count == 10)
                            {
                                GameHistory[item].RemoveAt(GameHistory[item].Count);
                            }
                        }

                        string startNumbers = Data.Split('$')[0];
                        string username = Data.Split('$')[1].Split('~')[0];
                        string moves = Data.Split('$')[1].Split('~')[1];
                        string time = Data.Split('$')[1].Split('~')[2];
                        string isFinished = Data.Split('$')[1].Split('~')[3];

                        string[] playerData = { username, moves, time, isFinished };

                        if (!GameHistory.ContainsKey(startNumbers))
                        {
                            GameHistory.Add(startNumbers, new List<string[]>());

                            SendLayout(startNumbers, GameHistory, clients);

                            GameHistory[startNumbers].Add(playerData);
                        }
                        else
                        {
                            var currentData = GameHistory[startNumbers].Where(stringToCheck => stringToCheck[0] == username).FirstOrDefault();

                            if (currentData != null)
                            {
                                if (currentData[3] == false.ToString())
                                {
                                    currentData[1] = moves;
                                    currentData[2] = time;
                                    currentData[3] = isFinished;
                                }

                                if (bool.Parse(isFinished) && TimeOnly.Parse(currentData[2]) >= TimeOnly.Parse(time) && int.Parse(currentData[1]) >= int.Parse(moves))
                                {
                                    currentData[1] = moves;
                                    currentData[2] = time;
                                }
                            }

                            if (currentData == null)
                            {
                                GameHistory[startNumbers].Add(playerData);
                            }
                        }

                        Console.WriteLine($"New data: {Data.Split('$')[1]}");

                        SendTopScores(startNumbers, GameHistory, clients);

                        response.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ProcessClientAsync: " + ex.Message);
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

            private async void SendTopScores(string layout, Dictionary<string, List<string[]>> GameHistory, List<TcpClient> clients)
            {
                try
                {
                    string feedback = "#updateTopScores&";

                    foreach (var item in GameHistory[layout])
                    {
                        feedback += layout + "$" + item[0] + "~" + item[1] + "~" + item[2] + "~" + item[3] + "|";
                    }

                    feedback += "\n";

                    foreach (var client in clients)
                    {
                        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(feedback));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendTopScores: " + ex.Message);
                }
            }

            private async void SendLayout(string layout, Dictionary<string, List<string[]>> GameHistory, List<TcpClient> clients)
            {
                try
                {
                    string feedback = "#updateLayouts&";

                    feedback += layout;

                    feedback += "\n";

                    foreach (var client in clients)
                    {
                        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(feedback));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendLayout: " + ex.Message);
                }
            }

            private async void SendAllLayouts(Dictionary<string, List<string[]>> GameHistory, List<TcpClient> clients)
            {
                try
                {
                    string feedback = "#updateLayouts&";

                    foreach (var layout in GameHistory.Keys)
                    {
                        feedback += layout + "|";
                    }

                    //Console.WriteLine(feedback);
                    feedback += "\n";

                    foreach (var client in clients)
                    {
                        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(feedback));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendAllLayouts: " + ex.Message);
                }
            }
        }
    }
}