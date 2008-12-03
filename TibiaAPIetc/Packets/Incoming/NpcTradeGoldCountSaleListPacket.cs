using System;
using System.Collections.Generic;
using Tibia.Objects;

namespace Tibia.Packets
{
    public class NpcTradeGoldCountSaleListPacket : Packet
    {
        private int goldCount;
        public int GoldCount
        {
            get { return goldCount; }
        }

        private List<TradeItem> saleItems;
        public List<TradeItem> SaleItems
        {
            get { return saleItems; }
        }

        public NpcTradeGoldCountSaleListPacket()
            : base()
        {
            type = PacketType.NpcTradeGoldCountSaleList;
            destination = PacketDestination.Client;
        }

        public NpcTradeGoldCountSaleListPacket( byte[] data)
            : this()
        {
            ParseData(data);
        }

        public new bool ParseData(byte[] packet)
        {
            if (base.ParseData(packet))
            {
                if (type != PacketType.NpcTradeGoldCountSaleList) return false;
                PacketBuilder p = new PacketBuilder( packet, 3);
                goldCount = p.GetLong();

                int saleItemCount = p.GetByte();
                saleItems = new List<TradeItem>(saleItemCount);

                for (int i = 0; i < saleItemCount; i++)
                {
                    int id = p.GetInt();
                    byte subtype = p.GetByte();
                    saleItems.Add(new TradeItem(id, subtype));
                }

                index = p.Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static NpcTradeGoldCountSaleListPacket Create( int goldCount, List<TradeItem> saleItems)
        {
            PacketBuilder p = new PacketBuilder(PacketType.NpcTradeGoldCountSaleList);
            p.AddLong(goldCount);
            p.AddByte((byte)saleItems.Count);

            foreach (TradeItem t in saleItems)
            {
                p.AddInt(t.Id);
                p.AddByte(t.SubType);
            }

            return new NpcTradeGoldCountSaleListPacket(p.GetPacket());
        }
    }
}