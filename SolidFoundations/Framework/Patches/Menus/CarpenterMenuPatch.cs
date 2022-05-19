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
            harmony.Patch(AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.tryToBuild), null), prefix: new HarmonyMethod(GetType(), nameof(TryToBuildPrefix)));
            harmony.Patch(AccessTools.Constructor(typeof(CarpenterMenu), new[] { typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(CarpenterMenuPostfix)));
        }

        private static bool SetNewActiveBlueprintPrefix(CarpenterMenu __instance, int ___currentBlueprintIndex, List<BluePrint> ___blueprints, ref Building ___currentBuilding, ref int ___price, ref string ___buildingName, ref string ___buildingDescription, ref List<Item> ___ingredients)
        {
            if (SolidFoundations.buildingManager.GetSpecificBuildingModel<ExtendedBuildingModel>(___blueprints[___currentBlueprintIndex].name) is ExtendedBuildingModel model && model is not null)
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

        private static bool TryToBuildPrefix(CarpenterMenu __instance, ref bool __result)
        {
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.CurrentBlueprint.name) is false)
            {
                return true;
            }

            // TODO: Replace Game1.getFarm() with flexible location, to enable building on the island farm
            __result = AttemptToBuildStructure(Game1.getFarm(), __instance.CurrentBlueprint);

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

        private static bool AttemptToBuildStructure(Farm farm, BluePrint blueprint)
        {
            Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
            if (!CanBuildHere(farm, blueprint, tileLocation))
            {
                return false;
            }

            var buildingModel = SolidFoundations.buildingManager.GetSpecificBuildingModel<ExtendedBuildingModel>(blueprint.name);
            if (buildingModel is null)
            {
                return false;
            }

            var customBuilding = new GenericBuilding(buildingModel, blueprint, tileLocation) { LocationName = farm.NameOrUniqueName };
            customBuilding.owner.Value = Game1.player.UniqueMultiplayerID;

            string finalCheckResult = customBuilding.isThereAnythingtoPreventConstruction(farm, tileLocation);
            if (finalCheckResult != null)
            {
                Game1.addHUDMessage(new HUDMessage(finalCheckResult, Color.Red, 3500f));
                return false;
            }
            for (int y = 0; y < blueprint.tilesHeight; y++)
            {
                for (int x = 0; x < blueprint.tilesWidth; x++)
                {
                    Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y);
                    farm.terrainFeatures.Remove(currentGlobalTilePosition);
                }
            }

            farm.buildings.Add(customBuilding);
            customBuilding.RefreshModel();
            customBuilding.performActionOnConstruction(farm);
            customBuilding.updateInteriorWarps();
            SolidFoundations.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(blueprint.displayName), blueprint.displayName, Game1.player.farmName);

            return true;
        }

        public static bool CanBuildHere(Farm farm, BluePrint blueprint, Vector2 tileLocation)
        {
            for (int y5 = 0; y5 < blueprint.tilesHeight; y5++)
            {
                for (int x2 = 0; x2 < blueprint.tilesWidth; x2++)
                {
                    farm.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y5));
                }
            }
            foreach (Point additionalPlacementTile in blueprint.additionalPlacementTiles)
            {
                int x5 = additionalPlacementTile.X;
                int y4 = additionalPlacementTile.Y;
                farm.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x5, tileLocation.Y + (float)y4));
            }
            for (int y3 = 0; y3 < blueprint.tilesHeight; y3++)
            {
                for (int x3 = 0; x3 < blueprint.tilesWidth; x3++)
                {
                    Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y3);
                    if (!farm.isBuildable(currentGlobalTilePosition2))
                    {
                        return false;
                    }
                    foreach (Farmer farmer in farm.farmers)
                    {
                        if (farmer.GetBoundingBox().Intersects(new Rectangle(x3 * 64, y3 * 64, 64, 64)))
                        {
                            return false;
                        }
                    }
                }
            }
            foreach (Point additionalPlacementTile2 in blueprint.additionalPlacementTiles)
            {
                int x4 = additionalPlacementTile2.X;
                int y2 = additionalPlacementTile2.Y;
                Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)x4, tileLocation.Y + (float)y2);
                if (!farm.isBuildable(currentGlobalTilePosition3))
                {
                    return false;
                }
                foreach (Farmer farmer2 in farm.farmers)
                {
                    if (farmer2.GetBoundingBox().Intersects(new Rectangle(x4 * 64, y2 * 64, 64, 64)))
                    {
                        return false;
                    }
                }
            }
            if (blueprint.humanDoor != new Point(-1, -1))
            {
                Vector2 doorPos = tileLocation + new Vector2(blueprint.humanDoor.X, blueprint.humanDoor.Y + 1);
                if (!farm.isBuildable(doorPos) && !farm.isPath(doorPos))
                {
                    return false;
                }
            }

            return true;
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
