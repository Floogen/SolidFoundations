using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingItemConversion : BuildingItemConversion
    {
        [ContentSerializer(Optional = true)]
        public int MinutesPerConversion = -1;
        internal int? MinutesRemaining;

        [ContentSerializer(Optional = true)]
        public bool RefreshMaxDailyConversions;
        internal int? CachedMaxDailyConversions;

        public new List<ExtendedAdditionalChopDrops> ProducedItems;
    }
}
