using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class ContainerItemAddPacket : Packet
    {
        private byte container;
        private Item item;
        public byte Container
        {
            get { return container; }
        }
        public Item Item
        {
            get { return item; }
        }
        public ContainerItemAddPacket()
            : base()
        {
            type = PacketType.ContainerItemAdd;
            destination = PacketDestination.Client;
        }
        public ContainerItemAddPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.ContainerItemAdd) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                container = p.GetByte();
                item = p.GetItem();
                /*
                Tibia.Objects.Container c = client.Inventory.GetContainer(container);
                byte slot = 0;
                if (c != null)
                {
                    slot = (byte)c.Amount;
                }

                item.Loc = new ItemLocation(container, slot);*/
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ContainerItemAddPacket Create( byte container, Item item)
        {
            PacketBuilder p = new PacketBuilder(PacketType.ContainerItemAdd);
            p.AddByte(container);
            p.AddItem(item);
            return new ContainerItemAddPacket(p.GetPacket());
        }
    }
}