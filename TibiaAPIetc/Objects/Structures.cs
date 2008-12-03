using System;
using System.Text;

namespace Tibia.Objects
{
    /// <summary>
    /// Represents a tile in the map structure
    /// </summary>
    public struct Tile
    {
        public Location Location;
        public uint Number;
        public uint Address;
        public uint Id;

        public Tile(uint n) : this(n, 0) { }
        public Tile(uint n, uint a) : this(n, a, new Location(), 0) { }
        public Tile(uint n, uint a, Location l, uint i)
        {
            Location = l;
            Number = n;
            Address = a;
            Id = i;
        }

        /// <summary>
        /// Returns the string representation of this struct.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(16);
            builder.Append("{ Number=" + Number.ToString() + ", ");
            builder.Append("Address=" + Address.ToString() + ", ");
            builder.Append("Id=" + Id.ToString() + " }");

            return builder.ToString();
        }
    }

    public struct Channel
    {
        public Packets.ChatChannel Id;
        public string Name;

        public Channel(Packets.ChatChannel id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
