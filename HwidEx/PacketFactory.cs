using ZephyrServerA.Packets;

namespace HwidEx
{
    public static class PacketFactory
    {
        public static byte[] LoginResponse(bool success)
        {
            var p = new ZWriter();
            p.WriteByte(0); //LOGIN RESPONSE OPCODE
            p.WriteBool(success);
            return p.ToArray();
        }

        public static byte[] Heartbeat()
        {
            var p = new ZWriter();
            p.WriteByte(1); //HEARTBEAT OPCODE
            return p.ToArray();
        }
    }
}
