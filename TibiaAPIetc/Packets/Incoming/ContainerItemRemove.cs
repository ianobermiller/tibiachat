using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class ContainerItemRemovePacket : Packet
    {
        private byte container;
        private byte slot;
        public byte Container
        {
            get { return container; }
        }
        public byte Slot
        {
            get { return slot; }
        }
        public ContainerItemRemovePacket()
            : base()
        {
            type = PacketType.ContainerItemRemove;
            destination = PacketDestination.Client;
        }
        public ContainerItemRemovePacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.ContainerItemRemove) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                container = p.GetByte();
                slot = p.GetByte();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ContainerItemRemovePacket Create( byte container, byte slot)
        {
            PacketBuilder p = new PacketBuilder(PacketType.ContainerItemRemove);
            p.AddByte(container);
            p.AddByte(slot);
            return new ContainerItemRemovePacket(p.GetPacket());
        }
    }
}