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
        public string NameOfBuildingToUpgrade { get; set; }
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

            // TODO: Look into possibility including this as a part of the framework?
            base.additionalPlacementTiles = new List<Microsoft.Xna.Framework.Point>();

            // Establish if the blueprint is an upgrade
            if (!String.IsNullOrEmpty(this.NameOfBuildingToUpgrade))
            {
                base.nameOfBuildingToUpgrade = String.Concat(building.Owner, "/", "/", this.NameOfBuildingToUpgrade);
            }

            // Set private fields via reflection
            BetterBuildings.modHelper.Reflection.GetField<Texture2D>(this, "texture").SetValue(building.Texture);
        }
    }
}
