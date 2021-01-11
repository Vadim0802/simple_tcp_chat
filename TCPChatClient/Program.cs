using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPChatClient
{
    internal static class Program
    {
        private static void Main()
        {
            var client = new TcpClient("127.0.0.1", 8888);
            
            using var stream = client.GetStream();
            try
            {
                new Thread(ReceiveMessage).Start(stream);
                
                while (true)
                {
                    var message = Console.ReadLine() ?? "";
                    var data = Encoding.UTF8.GetBytes(message);
                
                    stream.Write(data, 0 , data.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                client.Close();
                stream.Close();
            }
        }

        private static void ReceiveMessage(object stream)
        {
            var connectionStream = (NetworkStream) stream;
            while (true)
            {
                try
                {
                    var data = new byte[64];
                    var message = "";
                    
                    do
                    {
                        var bytes = connectionStream.Read(data, 0, data.Length);
                        message += Encoding.UTF8.GetString(data, 0, bytes);
                    } while (connectionStream.DataAvailable);

                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }
    }
}