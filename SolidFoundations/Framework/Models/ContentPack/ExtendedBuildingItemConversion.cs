﻿using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingItemConversion : BuildingItemConversion
    {
        public int MinutesPerConversion = -1;
        internal int? MinutesRemaining;

        internal bool ShouldTrackTime { get { return MinutesPerConversion >= 0; } }

        [Obsolete("No longer used. Use MinutesPerConversion instead.")]
        public bool RefreshMaxDailyConversions;

        public bool TakeOnlyRequiredFromStack { get; set; }

        public new List<ExtendedGenericSpawnItemDataWithCondition> ProducedItems
        {
            set
            {
                _producedItems = value;
                base.ProducedItems = _producedItems.ToList<GenericSpawnItemDataWithCondition>();
            }
            get
            {
                return _producedItems;
            }
        }
        private List<ExtendedGenericSpawnItemDataWithCondition> _producedItems = new List<ExtendedGenericSpawnItemDataWithCondition>();
    }
}
