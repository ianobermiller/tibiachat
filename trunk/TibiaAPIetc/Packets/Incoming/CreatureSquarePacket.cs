using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class CreatureSquarePacket : Packet
    {
        private int creatureId;
        private SquareColor color;

        public int CreatureId
        {
            get { return creatureId; }
        }

        public SquareColor Color
        {
            get { return color; }
        }

        public CreatureSquarePacket() : base()
        {
            type = PacketType.CreatureSquare;
            destination = PacketDestination.Client;
        }

        public CreatureSquarePacket( byte[] data)
            : this()
        {
            ParseData(data);
        }

        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.CreatureSquare) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                creatureId = p.GetLong();
                color = (SquareColor)p.GetByte();
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
