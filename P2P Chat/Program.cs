using System;

namespace P2P_Chat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select Mode: (1) Server (2) Client");
            string mode = Console.ReadLine();

            if (mode == "1")
            {
                Console.Write("Enter Server Name (This is only for local identification): ");
                string serverName = Console.ReadLine();  // Server name is only used locally.
                Server server = new Server(serverName);
                server.Start();
            }
            else if (mode == "2")
            {
                Console.Write("Enter Server IP Address: ");
                string ipAddress = Console.ReadLine();

                Console.Write("Enter Port: ");
                int port = int.Parse(Console.ReadLine());

                Console.Write("Enter Username: ");
                string username = Console.ReadLine();

                Console.Write("Enter Session Key: ");
                string sessionKey = Console.ReadLine();

                Client client = new Client(ipAddress, port, sessionKey, username);
                client.Start();
            }
        }
    }
}
