using SocketLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;

namespace HwidEx
{
    internal sealed class Server : IDisposable
    {
        public static Server Instance { get; set; }

        public Database Database
        {
            get
            {
                return m_database;
            }
        }

        private List<Client> m_clients;
        private ZAcceptor m_acceptor;
        private Timer m_pinger;
        private Database m_database;

        public Server(short port,string connectionString)
        {
            m_clients = new List<Client>();

            m_acceptor = new ZAcceptor(IPAddress.Any, port);
            m_acceptor.OnClientAccepted += (s) => new Client(s).WaitForData();

            m_pinger = new Timer(300000); //5 min
            m_pinger.Elapsed += (s, e) => Ping();

            m_database = new Database(connectionString);
        }

        public void AddClient(Client c)
        {
            Logger.Write(LogLevel.Connection, "Client {0} connected", c.Name);
            m_clients.Add(c);
            UpdateTitle();
        }
        public void RemoveClient(Client c)
        {
            Logger.Write(LogLevel.Connection, "Client {0} disconnected", c.Name);
            m_clients.Remove(c);
            UpdateTitle();
        }

        private void Ping()
        {
            if (m_clients.Count > 0)
            {
                Logger.Write(LogLevel.Info, "Pinger execute {0} clients", m_clients.Count);

                for (int i = m_clients.Count; i-- > 0; )
                    m_clients[i].SendPacket(PacketFactory.Heartbeat());
            }
        }

        private void UpdateTitle()
        {
            Console.Title = string.Concat("HwidEx - Clients: ", m_clients.Count);
        }

        public void Run()
        {
            Logger.InitConsole("BasicHwidServer");
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Logger.Exception((Exception)e.ExceptionObject);

            m_acceptor.Start();
            Logger.Write(LogLevel.Server, "Listening on port {0}", m_acceptor.Port);

            m_pinger.Start();

            while (true)
            {
                try
                {
                    string[] tokens = Console.ReadLine().Split(' ');

                    switch (tokens[0])
                    {
                        case "exit":
                            return;
                        case "clear":
                        case "cls":
                            Console.Clear();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }
            }
        }

        public void Dispose()
        {
            m_pinger.Stop();
            m_pinger.Dispose();

            m_acceptor.Dispose();

            for (int i = m_clients.Count; i-- > 0; )
                m_clients[i].Disconnect();
        }
    }
}
