using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Models.General.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class ResponseEvent
    {
        public string Text { get; set; }
        public InteractiveTile Action { get; set; }
    }
}
