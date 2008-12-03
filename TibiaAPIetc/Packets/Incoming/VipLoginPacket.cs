using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class VipLoginPacket: Packet
    {
        private int playerId;
        public int PlayerId
        {
            get { return playerId; }
        }
        
        public VipLoginPacket() : base()
        {
            type = PacketType.VipLogin;
            destination = PacketDestination.Client;
        }

        public VipLoginPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }

        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.VipLogin) return false;
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
