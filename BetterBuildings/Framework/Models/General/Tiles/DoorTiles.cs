using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General.Tiles
{
    public enum DoorType
    {
        Standard,
        Tunnel,
        Animal
    }

    public class DoorTiles
    {
        public DoorType Type { get; set; } = DoorType.Standard;
        public TileLocation EntranceTile { get; set; }
        public TileLocation ExitTile { get; set; }
        public List<TileLocation> Tiles { get; set; }
    }
}
