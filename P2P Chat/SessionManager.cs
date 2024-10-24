using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace P2P_Chat
{
    internal class SessionManager
    {
        private static Dictionary<string, List<NetworkStream>> sessionStreams = new Dictionary<string, List<NetworkStream>>();
        private static List<NetworkStream> adminStreams = new List<NetworkStream>();

        public static void HandleClient(NetworkStream stream, bool isServer)
        {
            byte[] buffer = new byte[1024];

            // First, read the username
            string username = ReadMessage(stream, buffer);
            Console.WriteLine($"Username received: {username}");

            // Then, read the session key
            string sessionKey = ReadMessage(stream, buffer);
            Console.WriteLine($"Session key received: {sessionKey}");

            Console.WriteLine($"Client '{username}' connected with session key: {sessionKey}\n");

            // Admin stream logic
            if (sessionKey == "Admin")
            {
                adminStreams.Add(stream);
                BroadcastMessage(stream, $"{username} (Admin) connected", "Admin");
            }
            else
            {
                // Regular session logic
                if (!sessionStreams.ContainsKey(sessionKey))
                {
                    sessionStreams[sessionKey] = new List<NetworkStream>();
                }
                sessionStreams[sessionKey].Add(stream);
            }

            // Start listening for chat messages
            while (true)
            {
                try
                {
                    string message = ReadMessage(stream, buffer);
                    if (string.IsNullOrEmpty(message)) break; // Client disconnected

                    if (isServer)
                    {
                        Console.WriteLine($"Received from {username} ({sessionKey}): {message}");
                    }

                    if (sessionKey == "Admin")
                    {
                        BroadcastMessage(stream, message, username);
                    }
                    else
                    {
                        // Send message to other clients in the same session
                        SendToSession(sessionKey, $"{username}: {message}", stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{username} disconnected. Error: {ex.Message}");
                    break;
                }
            }

            // Clean up client after disconnection
            if (sessionKey == "Admin")
            {
                adminStreams.Remove(stream);
            }
            else
            {
                sessionStreams[sessionKey].Remove(stream);
            }
        }

        public static void SendToSession(string sessionKey, string message, NetworkStream senderStream)
        {
            if (sessionStreams.ContainsKey(sessionKey))
            {
                foreach (var stream in sessionStreams[sessionKey])
                {
                    if (stream != senderStream) // Ensure the message is not sent back to the sender
                    {
                        try
                        {
                            byte[] data = Encoding.ASCII.GetBytes(message);
                            stream.Write(data, 0, data.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending message to session: {ex.Message}");
                        }
                    }
                }
            }
        }

        public static void BroadcastMessage(NetworkStream senderStream, string message, string username)
        {
            byte[] data = Encoding.ASCII.GetBytes($"{username} (Admin): {message}");

            // Send to all admin streams, excluding the sender
            foreach (var stream in adminStreams)
            {
                if (stream != senderStream)
                {
                    try
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting message to admins: {ex.Message}");
                    }
                }
            }

            // Send to all session streams, excluding the sender
            foreach (var session in sessionStreams)
            {
                foreach (var stream in session.Value)
                {
                    if (stream != senderStream)
                    {
                        try
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error broadcasting message to sessions: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static string ReadMessage(NetworkStream stream, byte[] buffer)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return bytesRead > 0 ? Encoding.ASCII.GetString(buffer, 0, bytesRead) : null;
        }
    }
}
