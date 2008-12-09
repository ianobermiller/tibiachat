using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tibia_Chat
{
    public class Vip
    {
        public string Name;
        public bool Online;
        public int Id;
        public Vip(int id, string name, bool online)
        {
            Name = name;
            Id = id;
            Online = online;
        }

        public static List<Vip> GetVips(byte[] packet)
        {
            List<Vip> vl = new List<Vip>();
            Tibia.Packets.PacketBuilder p = new Tibia.Packets.PacketBuilder(packet);
            for (int i = 0; i < p.Data.Length-4; i++)
            {
                p.Seek(i);
                byte b = p.GetByte();
                if (b == 0x8D)
                {
                    p.Skip(6);
                    if (p.GetByte() == 0xD2)
                    {
                        vl.Add(new Vip(p.GetLong(), p.GetString(), Convert.ToBoolean(p.GetByte())));
                        for (int j = 0; j < 99; j++)
                        {
                            if (p.GetByte() == 0xD2) vl.Add(new Vip(p.GetLong(), p.GetString(), Convert.ToBoolean(p.GetByte())));
                            else break;
                        }
                        break;
                    }                    
                }
            }
            return vl;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
