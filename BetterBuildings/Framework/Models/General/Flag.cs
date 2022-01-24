using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public enum FlagType
    {
        Temporary, // Removed on a new day start
        Permanent
    }

    public class Flag
    {
        public string Name { get; set; }
        public FlagType Type { get; set; } = FlagType.Temporary;
    }
}
