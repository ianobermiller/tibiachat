using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class PrivateChannelOpenPacket : Packet
    {
        private string name;
        public string Name
        {
            get { return name; }
        }
        public PrivateChannelOpenPacket()
            : base()
        {
            type = PacketType.PrivateChannelOpen;
            destination = PacketDestination.Client;
        }
        public PrivateChannelOpenPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.PrivateChannelOpen) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                name = p.GetString();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static PrivateChannelOpenPacket Create(string Name)
        {
            PacketBuilder p = new PacketBuilder(PacketType.PrivateChannelOpen);
            p.AddString(Name);
            return new PrivateChannelOpenPacket(p.GetPacket());
        }
    }
}