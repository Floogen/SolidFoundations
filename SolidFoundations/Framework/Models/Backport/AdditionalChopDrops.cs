using Microsoft.Xna.Framework.Content;

namespace SolidFoundations.Framework.Models.Backport
{
    // TODO: When updated to SDV v1.6, this class should be deleted in favor of using StardewValley.GameData.AdditionalChopDrops
    public class AdditionalChopDrops
    {
        public string ItemID;

        public int MinCount;

        public int MaxCount;

        [ContentSerializer(Optional = true)]
        public float Chance = 1f;

        [ContentSerializer(Optional = true)]
        public string Condition;
    }
}