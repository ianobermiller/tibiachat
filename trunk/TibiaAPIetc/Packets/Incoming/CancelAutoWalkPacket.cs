using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class CancelAutoWalkPacket : Packet
    {
        private byte dir;
        public byte Dir
        {
            get { return dir; }
        }

        public CancelAutoWalkPacket()
            : base()
        {
            type = PacketType.CancelAutoWalk;
            destination = PacketDestination.Client;
        }

        public CancelAutoWalkPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }

        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.CancelAutoWalk) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                dir = p.GetByte();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static CancelAutoWalkPacket Create(byte dir)
        {
            PacketBuilder p = new PacketBuilder(PacketType.CancelAutoWalk);
            p.AddByte(dir);
            return new CancelAutoWalkPacket(p.GetPacket());
        }
    }
}