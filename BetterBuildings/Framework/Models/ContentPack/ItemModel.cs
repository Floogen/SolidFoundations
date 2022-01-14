using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public enum Quality
    {
        Any,
        Normal,
        Silver,
        Gold,
        Iridium
    }

    public class ItemModel
    {
        public string Name { get; set; }
        public bool IsVanillaItem { get; set; } = true;
        public int Quantity { get; set; }
        public Quality Quality { get; set; }

        internal int GetActualQuality()
        {
            switch (Quality)
            {
                case Quality.Normal:
                    return 0;
                case Quality.Silver:
                    return 1;
                case Quality.Gold:
                    return 2;
                case Quality.Iridium:
                    return 4;
            }

            return -1;
        }
    }
}
