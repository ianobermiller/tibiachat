using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class EqItemAddPacket: Packet
    {
        private Item item;
        private Constants.SlotNumber slot;
        public Item Item
        {
            get { return item; }
        }
        public Constants.SlotNumber Slot
        {
            get { return slot; }
        }
        public EqItemAddPacket()
            : base()
        {
            type = PacketType.EqItemAdd;
            destination = PacketDestination.Client;
        }
        public EqItemAddPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.EqItemAdd) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                slot = (Tibia.Constants.SlotNumber)p.GetByte();
                item = p.GetItem();
                item.Loc = new ItemLocation(slot);
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }
        public static EqItemAddPacket Create( Item i, Tibia.Constants.SlotNumber slot)
        {
            PacketBuilder p = new PacketBuilder(PacketType.EqItemAdd);
            p.AddByte((byte)slot);
            p.AddItem(i);
            return new EqItemAddPacket(p.GetPacket());
        }
    }
}
