using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace P2P_Chat
{
    internal class Client
    {
        private string ipAddress;
        private int port;
        private string username;
        private string sessionKey;

        public Client(string ipAddress, int port, string sessionKey, string username)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.sessionKey = sessionKey;
            this.username = username;
        }

        public void Start()
        {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(ipAddress), port);
            NetworkStream stream = client.GetStream();

            // Send username as the first distinct message
            SendMessage(stream, username);
            Thread.Sleep(100);
            // Send session key as the second distinct message
            SendMessage(stream, sessionKey);

            // Start listening for incoming messages
            Thread receiveThread = new Thread(() => ReceiveMessages(stream));
            receiveThread.Start();

            // Start sending messages
            SendMessages(stream);
            client.Close();
        }

        private void SendMessages(NetworkStream stream)
        {
            while (true)
            {
                string messageToSend = Console.ReadLine();
                if (messageToSend == "exit") break;

                SendMessage(stream, messageToSend);
            }
        }
        private void SendMessage(NetworkStream stream, string message)
        {
            byte[] messageData = Encoding.ASCII.GetBytes(message);
            stream.Write(messageData, 0, messageData.Length);
        }
        private void ReceiveMessages(NetworkStream stream)
        {
            byte[] receivedBuffer = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                    if (bytesRead == 0) break;

                    string messageReceived = Encoding.ASCII.GetString(receivedBuffer, 0, bytesRead);
                    Console.WriteLine($"{messageReceived}");
                }
                catch
                {
                    Console.WriteLine("Disconnected from server.");
                    break;
                }
            }
        }

    }
}
