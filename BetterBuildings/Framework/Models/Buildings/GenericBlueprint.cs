using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public class GenericBlueprint : BluePrint
    {
        public List<Item> RequiredItems { get; set; }

        public GenericBlueprint() : base(ModDataKeys.GENERIC_BLUEPRINT)
        {

        }

        public GenericBlueprint(BuildingModel building) : this()
        {
            base.displayName = building.Name;
            base.name = building.Id;
            base.tilesHeight = building.Dimensions.Height;
            base.tilesWidth = building.Dimensions.Width;

            // TODO: Look into possibility including this as a part of the framework?
            base.additionalPlacementTiles = new List<Microsoft.Xna.Framework.Point>();

            // Set private fields via reflection
            BetterBuildings.modHelper.Reflection.GetField<Texture2D>(this, "texture").SetValue(building.Texture);
        }

        public GenericBlueprint(int requiredMoney, string nameOfBuildingToUpgrade, BuildingModel building) : this(building)
        {
            base.moneyRequired = requiredMoney;

            // Establish if the blueprint is an upgrade
            if (!String.IsNullOrEmpty(nameOfBuildingToUpgrade))
            {
                base.nameOfBuildingToUpgrade = String.Concat(building.Owner, "/", "/", nameOfBuildingToUpgrade);
            }
        }
    }
}
