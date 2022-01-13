using BetterBuildings.Framework.Models.Buildings;
using BetterBuildings.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
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
            harmony.Patch(AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.draw), new[] { typeof(SpriteBatch) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Constructor(typeof(CarpenterMenu), new[] { typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(CarpenterMenuPostfix)));
        }

        private static bool SetNewActiveBlueprintPrefix(CarpenterMenu __instance, int ___currentBlueprintIndex, List<BluePrint> ___blueprints, ref Building ___currentBuilding, ref int ___price, ref string ___buildingName, ref string ___buildingDescription, ref List<Item> ___ingredients)
        {
            if (__instance.CurrentBlueprint is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            var buildingModel = BetterBuildings.buildingManager.GetSpecificBuildingModel<GenericBuilding>(genericBlueprint.name);
            if (buildingModel is null)
            {
                return true;
            }

            // Set the building
            var customBuilding = new Building(genericBlueprint, Vector2.Zero);
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

        private static bool DrawPrefix(CarpenterMenu __instance, SpriteBatch b, bool ___onFarm, Building ___currentBuilding, Building ___buildingToMove, string ___hoverText, bool ___upgrading, bool ___painting, bool ___demolishing, bool ___moving)
        {
            if (!___onFarm || ___currentBuilding is null || !___currentBuilding.modData.ContainsKey(ModDataKeys.GENERIC_BUILDING))
            {
                return true;
            }

            // Confirm that the blueprint is a GenericBlueprint
            if (__instance.CurrentBlueprint is not GenericBlueprint genericBlueprint || Game1.currentLocation is not BuildableGameLocation buildableGameLocation)
            {
                return true;
            }

            // Draw banner
            string message = (___upgrading ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", genericBlueprint.NameOfBuildingToUpgrade) : (___demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : ((!___painting) ? Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation") : Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint"))));
            SpriteText.drawStringWithScrollBackground(b, message, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(message) / 2, 16);


            // Draw the building
            if (!___upgrading && !___demolishing && !___moving && !___painting)
            {
                Vector2 mousePositionTile2 = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
                for (int y4 = 0; y4 < genericBlueprint.tilesHeight; y4++)
                {
                    for (int x3 = 0; x3 < genericBlueprint.tilesWidth; x3++)
                    {
                        int sheetIndex3 = genericBlueprint.getTileSheetIndexForStructurePlacementTile(x3, y4);
                        Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float)x3, mousePositionTile2.Y + (float)y4);
                        if (!buildableGameLocation.isBuildable(currentGlobalTilePosition3))
                        {
                            sheetIndex3++;
                        }
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Rectangle(194 + sheetIndex3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
                    }
                }

                foreach (Point additionalPlacementTile in genericBlueprint.additionalPlacementTiles)
                {
                    int x4 = additionalPlacementTile.X;
                    int y3 = additionalPlacementTile.Y;
                    int sheetIndex4 = genericBlueprint.getTileSheetIndexForStructurePlacementTile(x4, y3);
                    Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float)x4, mousePositionTile2.Y + (float)y3);
                    if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition4))
                    {
                        sheetIndex4++;
                    }
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Rectangle(194 + sheetIndex4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
                }
            }
            else if (!___painting && ___moving && ___buildingToMove != null)
            {
                // TODO: Implement moving
            }
            Game1.EndWorldDrawInUI(b);

            // Draw the interactable UI
            __instance.cancelButton.draw(b);
            __instance.drawMouse(b);
            if (___hoverText.Length > 0)
            {
                IClickableMenu.drawHoverText(b, ___hoverText, Game1.dialogueFont);
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
