using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketLib
{
    public class ZSocket
    {
        public static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static IPEndPoint CreateEndpoint(string ip, int port)
        {
            return new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public static Task Connect(Socket socket, IPEndPoint endpoint)
        {
            return Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endpoint, null);
        }
    }
}
