using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Patches.Outliers
{
    internal class BluePrintPatch : PatchTemplate
    {
        private readonly Type _object = typeof(BluePrint);

        internal BluePrintPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(BluePrint.doesFarmerHaveEnoughResourcesToBuild), null), prefix: new HarmonyMethod(GetType(), nameof(DoesFarmerHaveEnoughResourcesToBuildPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(BluePrint.consumeResources), null), prefix: new HarmonyMethod(GetType(), nameof(ConsumeResourcesPrefix)));
            harmony.Patch(AccessTools.Constructor(typeof(BluePrint), new[] { typeof(string) }), prefix: new HarmonyMethod(GetType(), nameof(BluePrintPrefix)));
        }

        private static bool ConsumeResourcesPrefix(BluePrint __instance)
        {
            if (__instance is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            foreach (Object item in genericBlueprint.RequiredItems)
            {
                InventoryTools.ConsumeItemBasedOnQuantityAndQuality(item, item.Stack, item.Quality);
            }
            Game1.player.Money -= genericBlueprint.moneyRequired;

            return false;
        }

        private static bool DoesFarmerHaveEnoughResourcesToBuildPrefix(BluePrint __instance, ref bool __result)
        {
            if (__instance is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            __result = InventoryTools.HasRequiredItemsInInventory(genericBlueprint.RequiredItems);
            if (Game1.player.Money < genericBlueprint.moneyRequired)
            {
                __result = false;
            }

            return false;
        }

        private static bool BluePrintPrefix(BluePrint __instance, string name)
        {
            if (name == ModDataKeys.GENERIC_BLUEPRINT)
            {
                return false;
            }

            return true;
        }
    }
}
