using SocketLib;
using System;
using System.Net.Sockets;
using ZephyrServerA.Packets;

namespace HwidEx
{
    public sealed class Client : ZSession
    {
        public string Name { get; set; }

        private bool m_loggedIn;
        private string m_hwid;

        public Client(Socket socket)
            : base(socket)
        {
            Name = socket.RemoteEndPoint.ToString();
            Server.Instance.AddClient(this);
        }

        protected override void OnPacket(byte[] packet)
        {
            try
            {
                ZReader reader = new ZReader(packet);
                byte operation = reader.ReadByte();

                switch (operation)
                {
                    case 0: //LOGIN OPCODE

                        string user = reader.ReadMapleString();
                        string pass = reader.ReadMapleString();
                        m_hwid = reader.ReadMapleString();

                        m_loggedIn = Server.Instance.Database.Login(user, pass, m_hwid);

                        SendPacket(PacketFactory.LoginResponse(m_loggedIn));

                        Logger.Write(LogLevel.Info, "First packet ever!");
                        break;
                    case 1: //HEARTBEAT OPCODE

                        string tempHwid = reader.ReadMapleString();

                        if (tempHwid != m_hwid)
                            Disconnect();

                        break;
                    default:
                        Logger.Write(LogLevel.DataLoad, "Unknown packet {0}", BitConverter.ToString(packet));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                Disconnect();
            }
        }

        protected override void OnDisconnected()
        {
            Server.Instance.RemoveClient(this);
        }
    }
}
