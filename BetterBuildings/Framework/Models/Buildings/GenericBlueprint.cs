using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Models.Buildings
{
    public enum Vendor
    {
        Robin,
        Wizard
    }

    internal class GenericBlueprint : BluePrint
    {
        public Vendor Vendor { get; set; } = Vendor.Robin;
        public int RequiredMoney { get; set; }
        public List<GenericItem> RequiredItems { get; set; } = new List<GenericItem>();

        public GenericBlueprint() : base(ModDataKeys.GENERIC_BLUEPRINT)
        {

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

        internal void Setup(GenericBuilding building)
        {
            base.displayName = building.Name;
            base.name = building.Id;
            base.itemsRequired = new Dictionary<int, int>();
            base.moneyRequired = this.RequiredMoney;
            base.tilesHeight = building.Dimensions.Height;
            base.tilesWidth = building.Dimensions.Width;

            // Set private fields via reflection
            BetterBuildings.modHelper.Reflection.GetField<Texture2D>(this, "texture").SetValue(building.Texture);
        }
    }
}
