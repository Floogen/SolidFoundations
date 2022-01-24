using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public enum Operation
    {
        Add,
        Remove
    }

    public class ModifyFlagEvent
    {
        public string Name { get; set; }
        public FlagType Type { get; set; } = FlagType.Temporary;
        public Operation Operation { get; set; } = Operation.Add;
    }
}
