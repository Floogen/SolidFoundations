using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework;
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
    public class FactoryModel
    {
        public List<RecipeModel> Recipes { get; set; } = new List<RecipeModel>();

        public RecipeModel GetEligibleRecipe(List<Item> currentItems)
        {
            foreach (var recipe in Recipes)
            {
                bool hasAllNeededItems = true;
                foreach (Object item in InventoryTools.GetActualRequiredItems(recipe.InputItems))
                {
                    if (!InventoryTools.HasItemWithRequiredQuantityAndQuality(currentItems, item, item.Stack, item.Quality))
                    {
                        hasAllNeededItems = false;
                    }
                }

                if (hasAllNeededItems)
                {
                    return recipe;
                }
            }
            return null;
        }
    }
}
