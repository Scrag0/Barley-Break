using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Barley_Break
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private bool isConnected = false;

        public TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }
        public NetworkStream Stream { get => stream; set => stream = value; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }

        public void Connect(string host, string port)
        {
            try
            {
                TcpClient.Connect(IPAddress.Parse(host), int.Parse(port));
                stream = TcpClient.GetStream();
                IsConnected = true;
            }
            catch
            {
                MessageBox.Show("Error: Server is not connected");
            }
        }

        //метод, що від'єднує користувача з сервером
        public void Disconnect()
        {
            if (IsConnected)
            {
                Stream.Write(Encoding.UTF8.GetBytes("$Disconnect\n"));
                Stream.Close();
                TcpClient.Close();
                IsConnected = false;
            }
        }

        //метод, що відправляє нові результати
        public async void SendPlayerData(string startNumbers, string userName, int moves, string time)
        {
            try
            {
                string playerData = startNumbers + "$" + userName + "~" + moves + "~" + time;
                byte[] data = Encoding.UTF8.GetBytes(playerData + '\n');
                await Stream.WriteAsync(data);
            }
            catch
            {
                //ClearTopScores();
                //Print("Error: Send new player data");
            }
        }

        //метод, що отримує поточні результати гравців від сервера
        public string GetData()
        {
            string data = string.Empty;
            var response = new List<byte>();
            int bytesRead = 10;

            try
            {
                while ((bytesRead = Stream.ReadByte()) != '\n')
                {
                    response.Add((byte)bytesRead);
                }
                data = Encoding.UTF8.GetString(response.ToArray());

                response.Clear();
            }
            catch
            {
                //ClearTopScores();
                //Print("Error: Get scores");
            }
            return data;
        }
    }
}
