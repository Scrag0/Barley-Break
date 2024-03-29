﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Barley_Break
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private bool isConnected = false;
        private string exception = string.Empty;

        public TcpClient TcpClient { get => tcpClient; }
        public NetworkStream Stream { get => stream; }
        public bool IsConnected { get => isConnected; set => isConnected = value; }
        public string Exception { get => exception; set => exception = value; }

        public Client()
        {
            tcpClient = new TcpClient();
        }

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

        public async void SendPlayerData(string startNumbers, string userName, int moves, string time, string IsFinished)
        {
            try
            {
                string playerData = startNumbers + "$" + userName + "~" + moves + "~" + time + "~" + IsFinished;
                byte[] data = Encoding.UTF8.GetBytes(playerData + '\n');
                await Stream.WriteAsync(data);
            }
            catch (Exception ex)
            {
                Exception = "SendPlayerData: " + ex.Message;
                IsConnected = false;
            }
        }

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
            catch(Exception ex)
            {
                Exception = "GetData: " + ex.Message;
                IsConnected = false;
            }
            return data;
        }

        public string GetClientException()
        {
            string data = "#clientException&";

            if (string.IsNullOrEmpty(Exception)) return string.Empty;
            
            data += Exception;
            Exception = string.Empty;

            return data;
        }
    }
}
