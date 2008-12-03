using System;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class DefaultTemplatePacket : Packet
    {
        public DefaultTemplatePacket()
            : base()
        {
            type = PacketType.DefaultTemplate;
            destination = PacketDestination.Client;
        }
        public DefaultTemplatePacket( byte[] data)
            : this()
        {
            ParseData(data);
        }
        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.DefaultTemplate) return false;
                PacketBuilder p = new PacketBuilder(packet, 3);

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