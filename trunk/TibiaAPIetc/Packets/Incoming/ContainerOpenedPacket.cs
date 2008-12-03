using System;
using System.Collections.Generic;
using System.Text;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class ContainerOpenedPacket : Packet
    {
        int icon;
        byte number;
        string name;
        int volume;
        List<Item> items;
        int itemCount;

        #region Properties
        public int Icon
        {
            get { return icon; }
        }

        public byte Number
        {
            get { return number; }
        }

        public string Name
        {
            get { return name; }
        }

        public int Volume
        {
            get { return volume; }
        }

        public List<Item> Items
        {
            get { return items; }
        }
        #endregion

        public ContainerOpenedPacket() : base()
        {
            type = PacketType.ContainerOpened;
            destination = PacketDestination.Client;
        }
        public ContainerOpenedPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }

        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.ContainerOpened) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                number = p.GetByte();
                icon = p.GetInt();
                name = p.GetString();
                volume = p.GetInt();
                itemCount = p.GetByte();
                items = new List<Item>(itemCount);
                for (byte i = 0; i < itemCount; i++)
                {
                    Item item = p.GetItem();
                    item.Loc = new ItemLocation(number, i);
                    items.Add(item);
                }
                index = p.Index;
                return true;
            }
            else 
            {
                return false;
            }
        }


        public static ContainerOpenedPacket Create( int icon, byte number, string name, int volume, List<Item> items)
        {
            PacketBuilder p = new PacketBuilder(PacketType.ContainerOpened);
            short countable = 0;
            foreach (Item item in items)
            {
                if (item.Extra > 0)
                    countable++;
            }
            p.AddByte(number);
            p.AddInt(icon);
            p.AddString(name);
            p.AddInt(volume);
            p.AddByte((byte)items.Count);
            foreach (Item item in items)
            {
                p.AddItem(item);
            }

            return new ContainerOpenedPacket(p.GetPacket());
        }
    }
}
