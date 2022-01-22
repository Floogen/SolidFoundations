using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class OutputEvent
    {
        public bool AlwaysDisplayMenu { get; set; }
        public MessageEvent Message { get; set; }
    }
}
