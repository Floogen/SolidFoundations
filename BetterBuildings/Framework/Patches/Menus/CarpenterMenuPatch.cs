using BetterBuildings.Framework.Models.Buildings;
using BetterBuildings.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
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

namespace BetterBuildings.Framework.Patches.Menus
{
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
            _monitor.Log(___currentBlueprintIndex.ToString(), LogLevel.Debug);
            _monitor.Log(___blueprints[___currentBlueprintIndex].name, LogLevel.Debug);

            var blueprint = ___blueprints[___currentBlueprintIndex];
            if (blueprint is null || blueprint is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            var buildingModel = BetterBuildings.buildingManager.GetSpecificBuildingModel<GenericBuilding>(genericBlueprint.name);
            if (buildingModel is null)
            {
                return true;
            }

            // Set the building
            var customBuilding = new Building();
            customBuilding.modData[ModDataKeys.GENERIC_BUILDING] = buildingModel.Id;
            customBuilding.buildingType.Value = ModDataKeys.GENERIC_BUILDING;
            ___currentBuilding.texture = new Lazy<Texture2D>(delegate
            {
                return buildingModel.Texture;
            });
            ___currentBuilding = customBuilding;

            // Set the building related properties
            ___buildingName = buildingModel.Name;
            ___buildingDescription = buildingModel.Description;
            ___price = genericBlueprint.RequiredMoney;

            // Set the required items needed to build
            ___ingredients.Clear();
            foreach (var item in genericBlueprint.GetActualRequiredItems())
            {
                ___ingredients.Add(item);
            }

            return false;
        }

        private static void CarpenterMenuPostfix(CarpenterMenu __instance, ref List<BluePrint> ___blueprints)
        {
            foreach (var building in BetterBuildings.buildingManager.GetAllBuildingModels())
            {
                ___blueprints.Add(building.Blueprint);
                _monitor.Log(building.Blueprint.name, LogLevel.Debug);
            }
        }
    }
}
