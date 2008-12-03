using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class InformationBoxPacket : Packet
    {
        private string message;
        public string Message
        {
            get { return message; }
        }
        public InformationBoxPacket()
            : base()
        {
            type = PacketType.InformationBox;
            destination = PacketDestination.Client;
        }
        public InformationBoxPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.InformationBox) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                message = p.GetString();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static InformationBoxPacket Create(string message)
        {
            PacketBuilder p = new PacketBuilder(PacketType.InformationBox);
            p.AddString(message);
            return new InformationBoxPacket(p.GetPacket());
        }
    }
}