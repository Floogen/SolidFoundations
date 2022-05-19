using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace SolidFoundations.Framework.Models.Backport
{
    // TODO: When updated to SDV v1.6, this class should be deleted in favor of using StardewValley.GameData.IndoorItemAdd
    public class IndoorItemAdd
    {
        public string ItemID;

        public Point Tile;

        [ContentSerializer(Optional = true)]
        public bool Indestructible;
    }
}