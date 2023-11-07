using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingItemConversion : BuildingItemConversion
    {
        public int MinutesPerConversion = -1;
        internal int? MinutesRemaining;

        internal bool ShouldTrackTime { get { return MinutesPerConversion >= 0; } }

        [Obsolete("No longer used. Use MinutesPerConversion instead.")]
        public bool RefreshMaxDailyConversions;

        public new List<ExtendedGenericSpawnItemDataWithCondition> ProducedItems;
    }
}
