using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;

namespace Tibiaholic
{
    class DatFromItem
    {
        public int Flags;
        Client client;
        public uint DatAddress;
        public DatFromItem(Client client, uint id)
        {
            this.client = client;
            DatAddress = (uint)client.ReadInt(client.ReadInt(Tibia.Addresses.Client.DatPointer)
                + 8) + 0x4C * (id - 100);
            Flags = client.ReadInt(DatAddress + Tibia.Addresses.DatItem.Flags);
        }

        public void SetFlag(Tibia.Addresses.DatItem.Flag flag, bool on)
        {
            if (on)
                Flags |= (int)flag;
            else
                Flags &= ~(int)flag;
            client.WriteInt(DatAddress + Tibia.Addresses.DatItem.Flags, Flags);
        }

        public bool GetFlag(Tibia.Addresses.DatItem.Flag flag)
        {
            return (Flags & (int)flag) == (int)flag;
        }

        public Tibia.Addresses.DatItem.Help LensHelp
        {
            get { return (Tibia.Addresses.DatItem.Help)client.ReadInt(DatAddress + Tibia.Addresses.DatItem.LensHelp); }
            set { client.WriteInt(DatAddress + Tibia.Addresses.DatItem.LensHelp, (int)value); }
        }
    }
}
