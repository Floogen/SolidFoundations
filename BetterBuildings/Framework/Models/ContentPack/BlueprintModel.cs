using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public enum Vendor
    {
        Robin,
        Wizard
    }

    public class BlueprintModel
    {
        public Vendor Vendor { get; set; } = Vendor.Robin;
        public string NameOfBuildingToUpgrade { get; set; }
        public int DaysToConstruct { get; set; }
        public int RequiredMoney { get; set; }
        public List<ItemModel> RequiredItems { get; set; } = new List<ItemModel>();

        internal BuildingModel AssociatedBuildingModel { get; set; }

        internal GenericBlueprint CreateBlueprint()
        {
            var blueprint = new GenericBlueprint(DaysToConstruct, RequiredMoney, NameOfBuildingToUpgrade, AssociatedBuildingModel)
            {
                RequiredItems = InventoryTools.GetActualRequiredItems(this.RequiredItems)
            };

            return blueprint;
        }
    }
}
