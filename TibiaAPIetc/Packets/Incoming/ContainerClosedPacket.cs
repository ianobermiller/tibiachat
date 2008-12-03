using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class ContainerClosedPacket : Packet
    {
        private byte id;
        public byte Id
        {
            get { return id; }
        }

        public ContainerClosedPacket()
            : base()
        {
            type = PacketType.ContainerClosed;
            destination = PacketDestination.Client;
        }
        public ContainerClosedPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.ContainerClosed) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                id = p.GetByte();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ContainerClosedPacket Create(byte Id)
        {
            PacketBuilder p = new PacketBuilder(PacketType.ContainerClosed);
            p.AddByte(Id);
            return new ContainerClosedPacket(p.GetPacket());
        }
    }
}