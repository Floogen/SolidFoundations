using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class ModifyCutsceneSeenEvent
    {
        public int Id { get; set; }
        public Operation Operation { get; set; } = Operation.Add;
    }
}
