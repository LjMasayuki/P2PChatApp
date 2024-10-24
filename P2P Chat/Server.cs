using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace P2P_Chat
{
    internal class Server
    {
        private string serverName;  // Only for local identification
        private string ipAddress;
        private int port;

        public Server(string serverName)
        {
            this.serverName = serverName;  // Local use only, not involved in the client-to-server handshake
            this.ipAddress = Utils.GetLocalIPAddress();
            this.port = Utils.GetAvailablePort();
        }

        public void Start()
        {
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddress), port);
            server.Start();
            Console.WriteLine($"Server '{serverName}' started at IP: {ipAddress} and Port: {port}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Thread receiveThread = new Thread(() => HandleClient(stream));
                receiveThread.Start();
            }
        }

        private void HandleClient(NetworkStream stream)
        {
            SessionManager.HandleClient(stream, true);  // Handle the client communication (Username and Session Key)
        }
    }
}
