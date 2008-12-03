using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class CreatureSkullPacket : Packet
    {
        private int creatureId;
        private Constants.Skull skull;
        public int CreatureID
        {
            get { return creatureId; }
        }
        public Constants.Skull Skull
        {
            get { return skull; }
        }
        public CreatureSkullPacket()
            : base()
        {
            type = PacketType.CreatureSkull;
            destination = PacketDestination.Client;
        }
        public CreatureSkullPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.CreatureSkull) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                creatureId = p.GetLong();
                skull = (Constants.Skull)p.GetByte();
                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static CreatureSkullPacket Create(int id, Constants.Skull skull)
        {
            PacketBuilder p = new PacketBuilder(PacketType.CreatureSkull);
            p.AddLong(id);
            p.AddByte((byte)skull);
            return new CreatureSkullPacket(p.GetPacket());
        }
    }
}