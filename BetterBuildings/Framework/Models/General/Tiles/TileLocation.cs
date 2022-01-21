using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General.Tiles
{
    public class TileLocation
    {
        public int X { get; set; }
        public int Y { get; set; }

        public TileLocation GetAdjustedLocation(int xOffset, int yOffset)
        {
            return new TileLocation() { X = this.X + xOffset, Y = this.Y + yOffset };
        }

        public override bool Equals(object obj)
        {
            if (obj is TileLocation tileLocation && tileLocation is not null)
            {
                return this.X == tileLocation.X && this.Y == tileLocation.Y;
            }

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
    }
}
