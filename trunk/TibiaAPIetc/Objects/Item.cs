using System;
using System.Collections.Generic;
using System.IO;
using Tibia.Packets;

namespace Tibia.Objects
{
    public class Item
    {
        public ushort Id = 0;
        public bool HasExtraByte = false;
        public byte Extra=0;
        public ItemLocation Loc;

        public Item(ushort id) : this(id, 0) { }
        
        public Item(ushort id,byte extra)
        {
            FileStream fs = new FileStream("items.flag", FileMode.Open);
            byte[] buffer = new byte[3];
            fs.Seek((id - 100) * 3, SeekOrigin.Begin);
            buffer[0] = Convert.ToByte(fs.ReadByte());
            buffer[1] = Convert.ToByte(fs.ReadByte());
            buffer[2] = Convert.ToByte(fs.ReadByte());
            fs.Close();
            Id = id;
            HasExtraByte = Convert.ToBoolean(buffer[2]);
            if(HasExtraByte) Extra=extra;
        }   

    }

    /// <summary>
    /// Represents an item's location in game/memory. Can be either a slot, an inventory location, or on the ground.
    /// </summary>
    public class ItemLocation
    {
        public Constants.ItemLocationType type;
        public byte container;
        public byte position;
        public Location groundLocation;
        public byte stackOrder;
        public Constants.SlotNumber slot;

        /// <summary>
        /// Create a new item location at the specified slot (head, backpack, right, left, etc).
        /// </summary>
        /// <param name="s">slot</param>
        public ItemLocation(Constants.SlotNumber s)
        {
            type = Constants.ItemLocationType.Slot;
            slot = s;
        }

        /// <summary>
        /// Create a new item loction at the specified inventory location.
        /// </summary>
        /// <param name="c">container</param>
        /// <param name="p">position in container</param>
        public ItemLocation(byte c, byte p)
        {
            type = Constants.ItemLocationType.Container;
            container = c;
            position = p;
            stackOrder = p;
        }

        /// <summary>
        /// Create a new item location from a general location and stack order.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="stack"></param>
        public ItemLocation(Location l, byte stack)
        {
            type = Constants.ItemLocationType.Ground;
            groundLocation = l;
            stackOrder = stack;
        }

        /// <summary>
        /// Create a new item location at the specified location.
        /// </summary>
        /// <param name="l"></param>
        public ItemLocation(Location l)
        {
            type = Constants.ItemLocationType.Ground;
            groundLocation = l;
            stackOrder = 1;
        }

        /// <summary>
        /// Return a blank item location for packets (FF FF 00 00 00)
        /// </summary>
        /// <returns></returns>
        public static ItemLocation Hotkey()
        {
            return new ItemLocation(Constants.SlotNumber.None);
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[5];

            switch (type)
            {
                case Constants.ItemLocationType.Container:
                    bytes[00] = 0xFF;
                    bytes[01] = 0xFF;
                    bytes[02] = Convert.ToByte(0x40 + container);
                    bytes[03] = 0x00;
                    bytes[04] = position;
                    break;
                case Constants.ItemLocationType.Slot:
                    bytes[00] = 0xFF;
                    bytes[01] = 0xFF;
                    bytes[02] = Convert.ToByte(slot);
                    bytes[03] = 0x00;
                    bytes[04] = 0x00;
                    break;
                case Constants.ItemLocationType.Ground:
                    bytes[00] = Packet.Lo(groundLocation.X);
                    bytes[01] = Packet.Hi(groundLocation.X);
                    bytes[02] = Packet.Lo(groundLocation.Y);
                    bytes[03] = Packet.Hi(groundLocation.Y);
                    bytes[04] = Convert.ToByte(groundLocation.Z);
                    break;
            }

            return bytes;
        }
    }
}
