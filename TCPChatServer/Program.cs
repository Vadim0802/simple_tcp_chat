using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPChatServer
{
    internal class Client
    {
        public string Name { get; set; }
        public TcpClient ClientSocket { get; }
        public string Id { get; }

        public Client(TcpClient client, string id)
        {
            ClientSocket = client;
            Id = id;
        }
    }
    
    internal static class Program
    {
        private static readonly TcpListener ServerSocket = new TcpListener(IPAddress.Any, 8888);
        private static readonly List<Client> Clients = new List<Client>(); 
        
        private static void Main()
        {
            new Thread(Listen).Start();
        }

        private static void Listen()
        {
            ServerSocket.Start();

            while (true)
            {
                var connection = ServerSocket.AcceptTcpClient();

                var client = new Client(connection, Guid.NewGuid().ToString());

                Clients.Add(client);

                new Thread(Session).Start(client);
            }
        }

        private static void Session(object client)
        {
            var clientSocket = (Client) client;
            var clientStream = clientSocket.ClientSocket.GetStream();

            var userName = GetUserName(clientSocket.Id);
            clientSocket.Name = userName;
            
            while (true)
            {
                var buffer = new byte[256];
                var message = $"{clientSocket.Name}: ";

                try
                {
                    do
                    {
                        var bytes = clientStream.Read(buffer, 0, buffer.Length);
                        message += Encoding.UTF8.GetString(buffer, 0, bytes);
                    } while (clientStream.DataAvailable);

                    BroadcastMessage(message, clientSocket.Id);

                    Console.WriteLine(message);
                }
                catch
                {
                    break;
                }
            }
            RemoveClient(clientSocket.Id);
            clientSocket.ClientSocket?.Close();
            clientStream.Close();
        }

        private static void RemoveClient(string id)
        {
            var result = Clients.Find((client => client.Id == id));

            if (result != null)
                Clients.Remove(result);
        }
        
        private static void BroadcastMessage(string message, string id)
        {
            var data = Encoding.UTF8.GetBytes(message);
            
            Clients.ForEach((client) =>
            {
                if (client.Id == id) 
                    return;
                
                var stream = client.ClientSocket.GetStream();
                stream.Write(data, 0, data.Length);
            });
        }

        private static string GetUserName(string id)
        {
            var message = "Введите имя пользователя:";
            var data = Encoding.UTF8.GetBytes(message);

            var client = Clients.Find((item) => item.Id == id);

            var userName = "";
            
            if (client == null)
                return userName;
            
            var stream = client.ClientSocket.GetStream();
            
            stream.Write(data, 0, data.Length);
            
            var buffer = new byte[256];

            try
            {
                do
                {
                    var bytes = stream.Read(buffer, 0, buffer.Length);
                    userName += Encoding.UTF8.GetString(buffer, 0, bytes);
                } while (stream.DataAvailable);

                client.Name = userName;
            }
            catch
            {
                RemoveClient(client.Id);
                client.ClientSocket?.Close();
                stream.Close();
            }

            return userName;
        }
    }
}