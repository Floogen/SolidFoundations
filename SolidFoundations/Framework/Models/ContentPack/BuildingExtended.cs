using SolidFoundations.Framework.Models.Backport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    // TODO: When using SDV v1.6, this class should inherit StardewValley.GameData.BuildingData
    public class BuildingExtended : BuildingData
    {
        public bool IsLocked { get; set; }

        internal string Owner { get; set; }
        internal string PackName { get; set; }
    }
}
