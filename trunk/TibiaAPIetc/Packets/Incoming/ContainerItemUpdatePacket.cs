using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class ContainerItemUpdatePacket : Packet
    {
        private byte container;
        private byte slot;
        private Item item;
        public byte Container
        {
            get { return container; }
        }
        public byte Slot
        {
            get { return slot; }
        }
        public Item Item
        {
            get { return item; }
        }
        public ContainerItemUpdatePacket()
            : base()
        {
            type = PacketType.ContainerItemUpdate;
            destination = PacketDestination.Client;
        }
        public ContainerItemUpdatePacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.ContainerItemUpdate) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                container = p.GetByte();
                slot = p.GetByte();
                item = p.GetItem();
                item.Loc = new ItemLocation(container, slot);
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