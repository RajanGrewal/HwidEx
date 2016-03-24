using System;
using System.Net;
using System.Net.Sockets;

namespace SocketLib
{
    public class ZAcceptor
    {
        public short Port { get; private set; }

        private readonly TcpListener m_listener;

        private bool m_disposed;

        public event Action<Socket> OnClientAccepted;

        public ZAcceptor(IPAddress ip, short port)
        {
            Port = port;
            m_listener = new TcpListener(IPAddress.Any, port);

            OnClientAccepted = null;
            m_disposed = false;
        }

        public void Start()
        {
            m_listener.Start();
            m_listener.BeginAcceptSocket(EndAccept, null);
        }

        private void EndAccept(IAsyncResult iar)
        {
            if (m_disposed) { return; }

            Socket client = m_listener.EndAcceptSocket(iar);

            if (OnClientAccepted != null)
                OnClientAccepted(client);

            if (m_disposed) { return; }

            m_listener.BeginAcceptSocket(EndAccept, null);

        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                m_disposed = true;
                m_listener.Server.Close();
            }
        }
    }
}
