using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class BadLoginPacket : Packet
    {
        private string message;

        public string Message
        {
            get { return message; }
        }

        public BadLoginPacket() : base()
        {
            type = PacketType.BadLogin;
            destination = PacketDestination.Client;
        }
        public BadLoginPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.BadLogin) return false;
                PacketBuilder p = new PacketBuilder(packet, 3);
                int len = p.GetInt();
                p.Skip(1);
                message = p.GetString(len);
                index = p.Index;
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}
