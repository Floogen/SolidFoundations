using SolidFoundations.Framework.Models.Backport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    // TODO: When using SDV v1.6, this class should inherit StardewValley.GameData.BuildingData
    public class ExtendedBuildingModel : BuildingData
    {
        // TODO: When using SDV v1.6, likely flag this as obsolete in favor of StardewValley.GameData.BuildingData.BuildCondition?
        public bool IsLocked { get; set; }

        internal string Owner { get; set; }
        internal string PackName { get; set; }
    }
}
