using System;
using System.Net.Sockets;

namespace SocketLib
{
    public abstract class ZSession
    {
        public const int ReceiveSize = 256;
        public const int XorKey = 69;

        private Socket m_socket;
        private byte[] m_buffer;
        private byte[] m_packet;
        private int m_cursor;

        private bool m_connected;

        public bool Connected
        {
            get
            {
                return m_connected;
            }
        }

        public ZSession(Socket socket)
        {
            m_socket = socket;

            m_buffer = new byte[ReceiveSize];
            m_packet = new byte[ReceiveSize];

            m_connected = true;
        }

        protected abstract void OnPacket(byte[] packet);
        protected abstract void OnDisconnected();

        public void WaitForData()
        {
            if (!m_connected) { return; }

            SocketError error = SocketError.Success;

            m_socket.BeginReceive(m_buffer, 0, ReceiveSize, SocketFlags.None, out error, PacketCallback, null);

            if (error != SocketError.Success)
            {
                Disconnect();
            }
        }

        private void PacketCallback(IAsyncResult iar)
        {
            if (!m_connected) { return; }

            SocketError error = SocketError.Success;

            int length = m_socket.EndReceive(iar, out error);

            if (length == 0 || error != SocketError.Success)
            {
                Disconnect();
                return;
            }

            Append(length);

            while (m_cursor > 2)
            {
                ushort packetSize = BitConverter.ToUInt16(m_packet, 0);

                int segmentSize = packetSize + 2;

                if (m_cursor < segmentSize)
                    break;

                byte[] packetBuffer = new byte[packetSize];
                Buffer.BlockCopy(m_packet, 2, packetBuffer, 0, packetSize); //copy packet

                Cipher(packetBuffer, 0, packetBuffer.Length);

                m_cursor -= segmentSize; //fix len

                if (m_cursor > 0) //move reamining bytes
                    Buffer.BlockCopy(m_packet, segmentSize, m_packet, 0, m_cursor);

                OnPacket(packetBuffer);
            }

            WaitForData();
        }

        private void Append(int length)
        {
            if (m_packet.Length - m_cursor < length)
            {
                int newSize = m_packet.Length * 2;

                while (newSize < m_cursor + length)
                    newSize *= 2;

                Array.Resize<byte>(ref m_packet, newSize);
            }

            Buffer.BlockCopy(m_buffer, 0, m_packet, m_cursor, length);

            m_cursor += length;
        }

        public void SendPacket(byte[] data)
        {
            if (!m_connected) { return; }

            byte[] packet = new byte[data.Length + 2];
            byte[] header = BitConverter.GetBytes((ushort)data.Length);

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(data, 0, packet, 2, data.Length);

            Cipher(packet, 2, packet.Length);

            int offset = 0;

            while (offset < packet.Length)
            {

                SocketError errorCode = SocketError.Success;
                int sent = m_socket.Send(packet, offset, packet.Length - offset, SocketFlags.None, out errorCode);

                if (sent == 0 || errorCode != SocketError.Success)
                {
                    Disconnect();
                    return;
                }

                offset += sent;
            }
        }

        public void Disconnect()
        {
            if (m_connected)
            {
                m_connected = false;

                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                    m_socket.Disconnect(false);
                }
                finally
                {
                    m_socket.Dispose();


                    m_packet = null;
                    m_buffer = null;

                    OnDisconnected();
                }
            }
        }

        private static void Cipher(byte[] buffer, int start, int length)
        {
            for (int i = start; i < length; i++)
                buffer[i] = (byte)(buffer[i] ^ XorKey);
        }
    }
}
