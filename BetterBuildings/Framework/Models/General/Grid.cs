using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class Grid
    {
        public TileLocation StartingTile { get; set; } = new TileLocation();
        public int Height { get; set; }
        public int Width { get; set; }

        internal List<TileLocation> GetTiles()
        {
            var tiles = new List<TileLocation>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    tiles.Add(new TileLocation() { X = x, Y = y });
                }
            }

            return tiles;
        }
    }
}
