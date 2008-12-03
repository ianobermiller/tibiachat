using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class VipLogoutPacket : Packet
    {
        private int playerId;
        public int PlayerId
        {
            get { return playerId; }
        }

        public VipLogoutPacket()
            : base()
        {
            type = PacketType.VipLogout;
            destination = PacketDestination.Client;
        }

        public VipLogoutPacket(byte[] data)
            : this()
        {
            ParseData(data);
        }

        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.VipLogout) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                playerId = p.GetLong();
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
