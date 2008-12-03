using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class FlagUpdatePacket : Packet
    {
        public int flags;
        public bool HasFlag(Constants.Flag flag)
        {
            return (flags & (int)flag) == (int)flag;
        }
        public FlagUpdatePacket()
            : base()
        {
            type = PacketType.FlagUpdate;
            destination = PacketDestination.Client;
        }
        public FlagUpdatePacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.FlagUpdate) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                flags = p.GetInt();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }
        //TODO

        /*public static FlagUpdatePacket Create()
        {
            PacketBuilder p = new PacketBuilder(PacketType.FlagUpdate);
            
            return new FlagUpdatePacket(p.GetPacket());
        }*/
    }
}