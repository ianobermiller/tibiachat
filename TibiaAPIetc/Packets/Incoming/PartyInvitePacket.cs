using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class PartyInvitePacket : Packet
    {
        private int creatureId;
        private PartyType partytype;
        public int CreatureID
        {
            get { return creatureId; }
        }
        public PartyType PartyType
        {
            get { return partytype; }
        }
        public PartyInvitePacket()
            : base()
        {
            type = PacketType.PartyInvite;
            destination = PacketDestination.Client;
        }
        public PartyInvitePacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.PartyInvite) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                creatureId = p.GetLong();
                partytype = (PartyType)p.GetByte();
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