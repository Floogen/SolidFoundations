using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Utilities.Backport;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace SolidFoundations.Framework.Patches.Buildings
{
    // TODO: When updated to SDV v1.6, delete __instance patch
    internal class CarpenterMenuPatch : PatchTemplate
    {
        private readonly Type _object = typeof(CarpenterMenu);

        internal CarpenterMenuPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.setNewActiveBlueprint), null), prefix: new HarmonyMethod(GetType(), nameof(SetNewActiveBlueprintPrefix)));
            harmony.Patch(AccessTools.Constructor(typeof(CarpenterMenu), new[] { typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(CarpenterMenuPostfix)));
        }

        private static bool SetNewActiveBlueprintPrefix(CarpenterMenu __instance, int ___currentBlueprintIndex, List<BluePrint> ___blueprints, ref Building ___currentBuilding, ref int ___price, ref string ___buildingName, ref string ___buildingDescription, ref List<Item> ___ingredients)
        {
            if (SolidFoundations.buildingManager.GetSpecificBuildingModel<BuildingExtended>(___blueprints[___currentBlueprintIndex].name) is BuildingExtended model && model is not null)
            {
                Type buildingTypeFromName = GetBuildingTypeFromName(model.BuildingType);
                ___currentBuilding = new GenericBuilding(model, ___blueprints[___currentBlueprintIndex]);
            }
            else
            {
                return true;
            }

            ___price = ___blueprints[___currentBlueprintIndex].moneyRequired;
            ___ingredients.Clear();

            foreach (KeyValuePair<int, int> item in ___blueprints[___currentBlueprintIndex].itemsRequired)
            {
                ___ingredients.Add(new Object(item.Key, item.Value));
            }

            ___buildingDescription = ___blueprints[___currentBlueprintIndex].description;
            ___buildingName = ___blueprints[___currentBlueprintIndex].displayName;
            //__instance.UpdateAppearanceButtonVisibility();
            if (Game1.options.SnappyMenus && __instance.currentlySnappedComponent != null)// && __instance.currentlySnappedComponent == ___appearanceButton && !___appearanceButton.visible)
            {
                __instance.setCurrentlySnappedComponentTo(102);
                __instance.snapToDefaultClickableComponent();
            }

            return false;
        }

        private static void CarpenterMenuPostfix(CarpenterMenu __instance, ref List<BluePrint> ___blueprints, bool magicalConstruction = false)
        {
            string builder = "Carpenter";
            if (magicalConstruction)
            {
                builder = "Wizard";
            }

            foreach (var building in SolidFoundations.buildingManager.GetAllBuildingModels().Where(b => b.IsLocked is false))
            {
                if (String.IsNullOrEmpty(building.Builder) || building.Builder.Equals(builder, StringComparison.OrdinalIgnoreCase))
                {
                    ___blueprints.Add(new BluePrint(building.ID));
                }
            }
        }

        private static Type GetBuildingTypeFromName(string building_type_name)
        {
            Type type = null;
            if (building_type_name != null)
            {
                type = Type.GetType(building_type_name);
            }
            if (type == null)
            {
                type = typeof(Building);
            }
            return type;
        }

    }
}
