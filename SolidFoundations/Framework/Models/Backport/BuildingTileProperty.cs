using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SolidFoundations.Framework.Models.Backport
{
    // TODO: When updated to SDV v1.6, this class should be deleted in favor of using StardewValley.GameData.BuildingTileProperty
    public class BuildingTileProperty
    {
        public Point Tile;

        public string Key;

        [ContentSerializer(Optional = true)]
        public string Value;
    }
}