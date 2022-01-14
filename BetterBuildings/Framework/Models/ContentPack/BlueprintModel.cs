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
        public int RequiredMoney { get; set; }
        public List<ItemModel> RequiredItems { get; set; } = new List<ItemModel>();

        internal BuildingModel AssociatedBuildingModel { get; set; }

        internal GenericBlueprint CreateBlueprint()
        {
            var blueprint = new GenericBlueprint(RequiredMoney, NameOfBuildingToUpgrade, AssociatedBuildingModel)
            {
                RequiredItems = GetActualRequiredItems()
            };

            return blueprint;
        }

        public List<Item> GetActualRequiredItems()
        {
            List<Item> items = new List<Item>();
            foreach (var genericItem in RequiredItems)
            {
                KeyValuePair<int, string>? gameObjectData = Game1.objectInformation.FirstOrDefault(pair => pair.Value.Split('/')[0].Equals(genericItem.Name, StringComparison.OrdinalIgnoreCase));
                if (gameObjectData is not null)
                {
                    items.Add(new Object((int)(gameObjectData?.Key), genericItem.Quantity) { Quality = genericItem.GetActualQuality() });
                }
            }

            return items;
        }
    }
}
