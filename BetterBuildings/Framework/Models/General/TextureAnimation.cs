using BetterBuildings.Framework.Models.ContentPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class TextureAnimation
    {
        public int Frame { get; set; }
        public int RowOffset { get; set; }
        public int Duration { get; set; } = 1000;
        public bool OverrideStartingIndex { get; set; }
        public List<Condition> Conditions { get; set; }

        public bool PassesAllConditions(GenericBuilding customBuilding)
        {
            if (Conditions is null)
            {
                return true;
            }

            foreach (var condition in Conditions)
            {
                if (!condition.IsValid(customBuilding))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
