using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public enum StoreType
    {
        Vanilla,
        STF
    }

    public class ShopOpenEvent
    {
        public string Name { get; set; }
        public StoreType Type { get; set; } = StoreType.Vanilla;
    }
}
