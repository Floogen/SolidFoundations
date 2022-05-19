using System.Collections.Generic;

namespace SolidFoundations.Framework.Models.Backport
{
    // TODO: When updated to SDV v1.6, this class should be deleted in favor of using StardewValley.GameData.BuildingTileProperties
    public class BuildingTileProperties
    {
        public string LayerName;

        public List<BuildingTileProperty> Tiles;
    }
}