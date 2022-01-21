using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General.Tiles
{
    public class TileBase
    {
        public TileLocation Tile { get; set; }
        public Grid Grid { get; set; }


        public Grid GetActualGrid()
        {
            if (Grid is null)
            {
                return new Grid() { StartingTile = Tile, Height = 1, Width = 1 };
            }

            return Grid;
        }

        public List<TileLocation> GetActualTiles()
        {
            var tiles = new List<TileLocation>();
            if (Tile is not null)
            {
                tiles.Add(Tile);
            }
            else if (Grid is not null)
            {
                tiles = Grid.GetTiles();
            }

            return tiles;
        }
    }
}
