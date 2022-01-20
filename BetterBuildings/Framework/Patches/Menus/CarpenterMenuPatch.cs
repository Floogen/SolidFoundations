using BetterBuildings.Framework.Models.ContentPack;
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
            harmony.Patch(AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.tryToBuild), null), prefix: new HarmonyMethod(GetType(), nameof(TryToBuildPrefix)));
            harmony.Patch(AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.draw), new[] { typeof(SpriteBatch) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPrefix)));
            harmony.Patch(AccessTools.Constructor(typeof(CarpenterMenu), new[] { typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(CarpenterMenuPostfix)));

            harmony.CreateReversePatcher(AccessTools.Method(_object, nameof(FarmerRenderer.draw), new[] { typeof(SpriteBatch) }), new HarmonyMethod(GetType(), nameof(DrawReversePatch))).Patch();
        }

        private static bool SetNewActiveBlueprintPrefix(CarpenterMenu __instance, int ___currentBlueprintIndex, List<BluePrint> ___blueprints, ref Building ___currentBuilding, ref int ___price, ref string ___buildingName, ref string ___buildingDescription, ref List<Item> ___ingredients)
        {
            if (__instance.CurrentBlueprint is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            var buildingModel = BetterBuildings.buildingManager.GetSpecificBuildingModel<BuildingModel>(genericBlueprint.name);
            if (buildingModel is null)
            {
                return true;
            }

            // Set the building
            ___currentBuilding = buildingModel.CreateBuilding();

            // Set the building related properties
            ___buildingName = buildingModel.DisplayName;
            ___buildingDescription = buildingModel.Description;
            ___price = genericBlueprint.moneyRequired;

            // Set the required items needed to build
            ___ingredients.Clear();
            foreach (var item in genericBlueprint.RequiredItems)
            {
                ___ingredients.Add(item);
            }

            return false;
        }

        private static bool TryToBuildPrefix(CarpenterMenu __instance, ref bool __result)
        {
            if (__instance.CurrentBlueprint is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            // TODO: Replace Game1.getFarm() with flexible location, to enable building on the island farm
            __result = AttemptToBuildStructure(Game1.getFarm(), genericBlueprint);

            return false;
        }

        private static bool DrawPrefix(CarpenterMenu __instance, SpriteBatch b, bool ___onFarm, Building ___currentBuilding, Building ___buildingToMove, string ___buildingName, string ___buildingDescription, string ___hoverText, bool ___upgrading, bool ___painting, bool ___demolishing, bool ___moving, List<Item> ___ingredients)
        {
            // Confirm that the blueprint is a GenericBlueprint
            if (__instance.CurrentBlueprint is not GenericBlueprint genericBlueprint)
            {
                return true;
            }

            // Apply catalogue specific logic
            if (!___onFarm)
            {
                DrawActualMenu(__instance, b, ___currentBuilding, ___buildingName, ___buildingDescription, genericBlueprint.magical, ___ingredients, genericBlueprint.moneyRequired);
            }
            else
            {
                // Verify that we're working with a custom building
                if (___currentBuilding is null || !___currentBuilding.modData.ContainsKey(ModDataKeys.GENERIC_BUILDING))
                {
                    return true;
                }
                if (Game1.currentLocation is not BuildableGameLocation buildableGameLocation)
                {
                    return true;
                }

                // Draw banner
                string message = (___upgrading ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", genericBlueprint.nameOfBuildingToUpgrade) : (___demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : ((!___painting) ? Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation") : Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint"))));
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
            }

            // Draw the interactable UI
            __instance.cancelButton.draw(b);
            __instance.drawMouse(b);
            if (___hoverText.Length > 0)
            {
                IClickableMenu.drawHoverText(b, ___hoverText, Game1.dialogueFont);
            }

            return false;
        }

        private static void DrawActualMenu(CarpenterMenu menu, SpriteBatch b, Building currentBuilding, string buildingName, string buildingDescription, bool magicalConstruction, List<Item> ingredients, int price)
        {
            DrawReversePatch(menu, b);
            IClickableMenu.drawTextureBox(b, menu.xPositionOnScreen - 96, menu.yPositionOnScreen - 16, menu.maxWidthOfBuildingViewer + 64, menu.maxHeightOfBuildingViewer + 64, magicalConstruction ? Color.RoyalBlue : Color.White);
            currentBuilding.drawInMenu(b, menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer / 2 - currentBuilding.tilesWide.Value * 64 / 2 - 64, menu.yPositionOnScreen + menu.maxHeightOfBuildingViewer / 2 - currentBuilding.getSourceRectForMenu().Height * 4 / 2);
            if (menu.CurrentBlueprint.isUpgrade())
            {
                menu.upgradeIcon.draw(b);
            }
            string placeholder = " Deluxe  Barn   ";
            if (SpriteText.getWidthOfString(buildingName) >= SpriteText.getWidthOfString(placeholder))
            {
                placeholder = buildingName + " ";
            }
            SpriteText.drawStringWithScrollCenteredAt(b, buildingName, menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder - 16 + 64 + (menu.width - (menu.maxWidthOfBuildingViewer + 128)) / 2, menu.yPositionOnScreen, SpriteText.getWidthOfString(placeholder));
            int descriptionWidth = LocalizedContentManager.CurrentLanguageCode switch
            {
                LocalizedContentManager.LanguageCode.es => menu.maxWidthOfDescription + 64 + ((menu.CurrentBlueprint?.name == "Deluxe Barn") ? 96 : 0),
                LocalizedContentManager.LanguageCode.it => menu.maxWidthOfDescription + 96,
                LocalizedContentManager.LanguageCode.fr => menu.maxWidthOfDescription + 96 + ((menu.CurrentBlueprint?.name == "Slime Hutch" || menu.CurrentBlueprint?.name == "Deluxe Coop" || menu.CurrentBlueprint?.name == "Deluxe Barn") ? 72 : 0),
                LocalizedContentManager.LanguageCode.ko => menu.maxWidthOfDescription + 96 + ((menu.CurrentBlueprint?.name == "Slime Hutch") ? 64 : ((menu.CurrentBlueprint?.name == "Deluxe Coop") ? 96 : ((menu.CurrentBlueprint?.name == "Deluxe Barn") ? 112 : ((menu.CurrentBlueprint?.name == "Big Barn") ? 64 : 0)))),
                _ => menu.maxWidthOfDescription + 64,
            };
            IClickableMenu.drawTextureBox(b, menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer - 16, menu.yPositionOnScreen + 80, descriptionWidth, menu.maxHeightOfBuildingViewer - 32, magicalConstruction ? Color.RoyalBlue : Color.White);
            if (magicalConstruction)
            {
                Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer - 4, menu.yPositionOnScreen + 80 + 16 + 4), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
                Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer - 1, menu.yPositionOnScreen + 80 + 16 + 4), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f);
            }
            Utility.drawTextWithShadow(b, Game1.parseText(buildingDescription, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer, menu.yPositionOnScreen + 80 + 16), magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.75f);
            Vector2 ingredientsPosition = new Vector2(menu.xPositionOnScreen + menu.maxWidthOfBuildingViewer + 16, menu.yPositionOnScreen + 256 + 32);
            if (ingredients.Count < 3 && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt))
            {
                ingredientsPosition.Y += 64f;
            }
            if (price >= 0)
            {
                SpriteText.drawString(b, "$", (int)ingredientsPosition.X, (int)ingredientsPosition.Y);
                string price_string = Utility.getNumberWithCommas(price);
                if (magicalConstruction)
                {
                    Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f, ingredientsPosition.Y + 8f), Game1.textColor * 0.5f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
                    Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f - 1f, ingredientsPosition.Y + 8f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
                }
                Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f, ingredientsPosition.Y + 4f), (Game1.player.Money < price) ? Color.Red : (magicalConstruction ? Color.PaleGoldenrod : Game1.textColor), 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
            }
            ingredientsPosition.X -= 16f;
            ingredientsPosition.Y -= 21f;
            foreach (Item i in ingredients)
            {
                ingredientsPosition.Y += 68f;
                i.drawInMenu(b, ingredientsPosition, 1f);
                bool hasItem = ((!(i is StardewValley.Object) || Game1.player.hasItemInInventory((i as StardewValley.Object).parentSheetIndex, i.Stack)) ? true : false);
                if (magicalConstruction)
                {
                    Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 12f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
                    Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f - 1f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
                }
                Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f, ingredientsPosition.Y + 20f), hasItem ? (magicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
            }

            menu.backButton.draw(b);
            menu.forwardButton.draw(b);
            menu.okButton.draw(b, menu.CurrentBlueprint.doesFarmerHaveEnoughResourcesToBuild() ? Color.White : (Color.Gray * 0.8f), 0.88f);
            menu.demolishButton.draw(b, menu.CanDemolishThis(menu.CurrentBlueprint) ? Color.White : (Color.Gray * 0.8f), 0.88f);
            menu.moveButton.draw(b);
            menu.paintButton.draw(b);
        }

        private static bool AttemptToBuildStructure(Farm farm, GenericBlueprint blueprint)
        {
            Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
            if (!CanBuildHere(farm, blueprint, tileLocation))
            {
                return false;
            }

            var buildingModel = BetterBuildings.buildingManager.GetSpecificBuildingModel<BuildingModel>(blueprint.name);
            if (buildingModel is null)
            {
                return false;
            }

            var customBuilding = buildingModel.CreateBuilding(farm, tileLocation);
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
            BetterBuildings.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(blueprint.displayName), blueprint.displayName, Game1.player.farmName);

            return true;
        }

        public static bool CanBuildHere(Farm farm, GenericBlueprint blueprint, Vector2 tileLocation)
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

        private static void CarpenterMenuPostfix(CarpenterMenu __instance, ref List<BluePrint> ___blueprints)
        {
            foreach (var building in BetterBuildings.buildingManager.GetAllBuildingModels())
            {
                ___blueprints.Add(building.Blueprint.CreateBlueprint());
            }
        }

        private static void DrawReversePatch(CarpenterMenu __instance, SpriteBatch b)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
