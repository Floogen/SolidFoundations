using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class ModifyMailReceivedEvent
    {
        public string Name { get; set; }
        public Operation Operation { get; set; } = Operation.Add;
    }
}
