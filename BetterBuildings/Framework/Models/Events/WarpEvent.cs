using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class WarpEvent
    {
        public string Map { get; set; }
        public TileLocation DestinationTile { get; set; }
    }
}
