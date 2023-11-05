using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Extensions;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace SolidFoundations.Framework.Patches.Buildings
{
    internal class BuildingPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Building);

        internal BuildingPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Building.doAction), new[] { typeof(Vector2), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DoActionPrefix)));
            harmony.Patch(AccessTools.Method(_object, "CheckItemConversionRule", new[] { typeof(BuildingItemConversion), typeof(ItemQueryContext) }), prefix: new HarmonyMethod(GetType(), nameof(CheckItemConversionRulePrefix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Building.performActionOnDemolition), new[] { typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(PerformActionOnDemolitionPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.OnEndMove), null), postfix: new HarmonyMethod(GetType(), nameof(OnEndMovePostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.isActionableTile), new[] { typeof(int), typeof(int), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(IsActionableTilePostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.IsValidObjectForChest), new[] { typeof(Item), typeof(Chest) }), postfix: new HarmonyMethod(GetType(), nameof(IsValidObjectForChestPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.updateInteriorWarps), new[] { typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateInteriorWarpsPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.Update), new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdatePostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.performTenMinuteAction), new[] { typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(PerformTenMinuteActionPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Building.draw), new[] { typeof(SpriteBatch) }), transpiler: new HarmonyMethod(GetType(), nameof(DrawTranspiler)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.drawBackground), new[] { typeof(SpriteBatch) }), transpiler: new HarmonyMethod(GetType(), nameof(DrawBackgroundTranspiler)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.drawInMenu), new[] { typeof(SpriteBatch), typeof(int), typeof(int) }), transpiler: new HarmonyMethod(GetType(), nameof(DrawMenuTranspiler)));
            harmony.Patch(AccessTools.Method(_object, nameof(Building.drawInConstruction), new[] { typeof(SpriteBatch) }), transpiler: new HarmonyMethod(GetType(), nameof(DrawInConstructionTranspiler)));

            harmony.Patch(AccessTools.Constructor(_object, null), postfix: new HarmonyMethod(GetType(), nameof(BuildingPostfix)));
        }

        private static bool DoActionPrefix(Building __instance, Vector2 tileLocation, Farmer who, ref bool __result)
        {
            // Check if type is one of the extended models
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.buildingType.Value) is false)
            {
                return true;
            }
            var extendedModel = SolidFoundations.buildingManager.GetSpecificBuildingModel(__instance.buildingType.Value);

            if (who.isRidingHorse())
            {
                __result = false;
                return false;
            }
            if (who.IsLocalPlayer && tileLocation.X >= (float)(int)__instance.tileX.Value && tileLocation.X < (float)((int)__instance.tileX.Value + (int)__instance.tilesWide.Value) && tileLocation.Y >= (float)(int)__instance.tileY.Value && tileLocation.Y < (float)((int)__instance.tileY.Value + (int)__instance.tilesHigh.Value) && (int)__instance.daysOfConstructionLeft.Value > 0)
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
            }
            else
            {
                GameLocation interior = __instance.GetIndoors();
                if (who.IsLocalPlayer && (__instance.IsAuxiliaryTile(tileLocation) || tileLocation.X == (float)(__instance.humanDoor.X + (int)__instance.tileX.Value) && tileLocation.Y == (float)(__instance.humanDoor.Y + (int)__instance.tileY.Value) && interior != null))
                {
                    if (who.mount != null)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));

                        __result = false;
                        return false;
                    }
                    if (who.team.demolishLock.IsLocked())
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));

                        __result = false;
                        return false;
                    }
                    if (__instance.OnUseHumanDoor(who))
                    {
                        who.currentLocation.playSound("doorClose", tileLocation);
                        bool isStructure = __instance.indoors.Value != null;
                        Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
                    }

                    __result = true;
                    return false;
                }

                BuildingData data = __instance.GetData();
                if (data != null)
                {
                    Microsoft.Xna.Framework.Rectangle door = __instance.getRectForAnimalDoor(data);
                    door.Width /= 64;
                    door.Height /= 64;
                    door.X /= 64;
                    door.Y /= 64;
                    if ((int)__instance.daysOfConstructionLeft.Value <= 0 && door != Microsoft.Xna.Framework.Rectangle.Empty && door.Contains(Utility.Vector2ToPoint(tileLocation)) && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
                    {
                        __instance.ToggleAnimalDoor(who);

                        __result = true;
                        return false;
                    }
                    if (who.IsLocalPlayer && __instance.IsInTilePropertyRadius(tileLocation, checkAdditionalRadius: true) && !__instance.isTilePassable(tileLocation))
                    {
                        Point actualTile = new Point((int)tileLocation.X - __instance.tileX.Value, (int)tileLocation.Y - __instance.tileY.Value);
                        var specialActionAtTile = extendedModel.GetSpecialActionAtTile(actualTile.X, actualTile.Y);
                        if (specialActionAtTile is not null)
                        {
                            specialActionAtTile.Trigger(who, __instance, actualTile);

                            __result = true;
                            return false;
                        }

                        if (who.ActiveObject is not null && extendedModel.LoadChestTiles is not null && extendedModel.GetLoadChestActionAtTile(actualTile.X, actualTile.Y) is var loadChestName && String.IsNullOrEmpty(loadChestName) is false)
                        {
                            __instance.PerformBuildingChestAction(loadChestName, who);

                            __result = true;
                            return false;
                        }
                        if (who.ActiveObject is null && extendedModel.CollectChestTiles is not null && extendedModel.GetCollectChestActionAtTile(actualTile.X, actualTile.Y) is var collectChestName && String.IsNullOrEmpty(collectChestName) is false)
                        {
                            __instance.PerformBuildingChestAction(collectChestName, who);

                            __result = true;
                            return false;
                        }

                        string tileAction = data.GetActionAtTile((int)tileLocation.X - __instance.tileX.Value, (int)tileLocation.Y - __instance.tileY.Value);
                        if (tileAction != null)
                        {
                            tileAction = TokenParser.ParseText(tileAction);
                            if (who.currentLocation.performAction(tileAction, who, new Location((int)tileLocation.X, (int)tileLocation.Y)))
                            {
                                __result = true;
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    if (who.IsLocalPlayer && !__instance.isTilePassable(tileLocation) && Building.TryPerformObeliskWarp(__instance.buildingType.Value, who))
                    {
                        __result = true;
                        return false;
                    }
                    if (who.IsLocalPlayer && who.ActiveObject != null && !__instance.isTilePassable(tileLocation))
                    {
                        return __instance.performActiveObjectDropInAction(who, probe: false);
                    }
                }
            }

            return false;
        }

        private static bool CheckItemConversionRulePrefix(Building __instance, ExtendedBuildingItemConversion conversion, ItemQueryContext itemQueryContext)
        {
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.buildingType.Value) is false)
            {
                return true;
            }
            __instance.ProcessItemConversion(conversion, itemQueryContext);

            return false;
        }

        private static void PerformActionOnDemolitionPostfix(Building __instance, GameLocation location)
        {
            foreach (var lightSource in __instance.GetLightSources())
            {
                if (location.hasLightSource(lightSource.Identifier))
                {
                    location.removeLightSource(lightSource.Identifier);
                }
            }
        }

        private static void OnEndMovePostfix(Building __instance)
        {
            __instance.ResetLights();
        }

        private static void IsActionableTilePostfix(Building __instance, int xTile, int yTile, Farmer who, ref bool __result)
        {
            if (__instance.IsAuxiliaryTile(new Vector2(xTile, yTile)))
            {
                __result = true;
            }
        }
        
        private static void IsValidObjectForChestPostfix(Building __instance, Item item, Chest chest, ref bool __result)
        {
            if (__result is true && __instance.IsValidObjectForChest(item, chest))
            {
                __result = true;
            }
        }
        
        private static void UpdateInteriorWarpsPostfix(Building __instance, GameLocation interior = null)
        {
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.buildingType.Value) is false)
            {
                return;
            }
            var extendedModel = SolidFoundations.buildingManager.GetSpecificBuildingModel(__instance.buildingType.Value);

            interior = interior ?? __instance.GetIndoors();
            if (interior == null)
            {
                return;
            }

            GameLocation parentLocation = __instance.GetParentLocation();
            var baseX = __instance.humanDoor.X;
            var baseY = __instance.humanDoor.Y;

            if (extendedModel.TunnelDoors.Count > 0)
            {
                var firstTunnelDoor = extendedModel.TunnelDoors.First();
                baseX = firstTunnelDoor.X;
                baseY = firstTunnelDoor.Y;
            }

            foreach (Warp warp in interior.warps)
            {
                if (parentLocation != null)
                {
                    warp.TargetName = parentLocation.NameOrUniqueName;
                }

                warp.TargetX = baseX + __instance.tileX.Value;
                warp.TargetY = baseY + __instance.tileY.Value + 1;
            }
        }
        
        private static void UpdatePostfix(Building __instance, GameTime time)
        {
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.buildingType.Value) is false)
            {
                return;
            }
            var extendedModel = SolidFoundations.buildingManager.GetSpecificBuildingModel(__instance.buildingType.Value);

            // Handle lights
            var parentLocation = __instance.GetParentLocation();
            if (extendedModel.Lights is not null && __instance.GetParentLocation() is not null && parentLocation.sharedLights is not null)
            {
                var startingTile = new Point(__instance.tileX.Value, __instance.tileY.Value);

                // Add the required lights
                int lightCount = 0;
                foreach (var lightModel in extendedModel.Lights)
                {
                    lightCount++;

                    lightModel.ElapsedMilliseconds += time.ElapsedGameTime.Milliseconds;
                    if (lightModel.ElapsedMilliseconds < lightModel.GetUpdateInterval())
                    {
                        continue;
                    }
                    lightModel.GetUpdateInterval(recalculateValue: true);
                    lightModel.ElapsedMilliseconds = 0f;

                    var lightTilePosition = lightModel.Tile + startingTile;
                    int lightIdentifier = Toolkit.GetLightSourceIdentifierForBuilding(startingTile, lightCount);
                    if (parentLocation.hasLightSource(lightIdentifier) is false)
                    {
                        continue;
                    }

                    parentLocation.sharedLights[lightIdentifier].radius.Value = lightModel.GetRadius();
                }
            }

            // Catch touch actions
            if (parentLocation is not null)
            {
                Vector2 playerStandingPosition = new Vector2(Game1.player.StandingPixel.X / 64, Game1.player.StandingPixel.Y / 64);
                if (parentLocation.lastTouchActionLocation.Equals(Vector2.Zero))
                {
                    Point actualTile = new Point((int)playerStandingPosition.X - __instance.tileX.Value, (int)playerStandingPosition.Y - __instance.tileY.Value);
                    if (extendedModel.TunnelDoors.Any(d => d.X == actualTile.X && d.Y == actualTile.Y))
                    {
                        parentLocation.lastTouchActionLocation = new Vector2(Game1.player.StandingPixel.X / 64, Game1.player.StandingPixel.Y / 64);
                        bool isStructure = false;
                        if (__instance.indoors.Value is not null)
                        {
                            isStructure = true;
                        }
                        Game1.warpFarmer(__instance.indoors.Name, __instance.indoors.Value.warps[0].X, __instance.indoors.Value.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
                    }

                    var specialActionAtTile = extendedModel.GetSpecialEventAtTile(actualTile.X, actualTile.Y);
                    if (specialActionAtTile is not null)
                    {
                        parentLocation.lastTouchActionLocation = new Vector2(Game1.player.StandingPixel.X / 64, Game1.player.StandingPixel.Y / 64);
                        specialActionAtTile.Trigger(Game1.player, __instance, actualTile);
                    }
                    else
                    {
                        string eventTile = extendedModel.GetEventAtTile((int)playerStandingPosition.X - __instance.tileX.Value, (int)playerStandingPosition.Y - __instance.tileY.Value);
                        if (eventTile != null)
                        {
                            parentLocation.lastTouchActionLocation = new Vector2(Game1.player.StandingPixel.X / 64, Game1.player.StandingPixel.Y / 64);

                            eventTile = TokenParser.ParseText(eventTile);
                            eventTile = SolidFoundations.modHelper.Reflection.GetMethod(new Dialogue(null, string.Empty, eventTile), "checkForSpecialCharacters").Invoke<string>(eventTile);
                            if (parentLocation.performAction(eventTile, Game1.player, new xTile.Dimensions.Location((int)parentLocation.lastTouchActionLocation.X, (int)parentLocation.lastTouchActionLocation.Y)) is false)
                            {
                                parentLocation.performTouchAction(eventTile, playerStandingPosition);
                            }
                        }
                    }
                }
                else if (!parentLocation.lastTouchActionLocation.Equals(playerStandingPosition))
                {
                    parentLocation.lastTouchActionLocation = Vector2.Zero;
                }
            }
        }

        private static void PerformTenMinuteActionPostfix(Building __instance, int timeElapsed)
        {
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.buildingType.Value) is false)
            {
                return;
            }

            BuildingData data = __instance.GetData();
            if (data is null || !(data.ItemConversions?.Count > 0))
            {
                return;
            }

            ItemQueryContext itemQueryContext = new ItemQueryContext(__instance.GetParentLocation(), null, null);
            foreach (ExtendedBuildingItemConversion conversion in data.ItemConversions)
            {
                if (conversion.ShouldTrackTime is false)
                {
                    continue;
                }

                __instance.ProcessItemConversion(conversion, itemQueryContext, minutesElapsed: timeElapsed);
            }
        }
        
        private static bool ValidateConditionsForDrawLayer(Building building, ExtendedBuildingDrawLayer layer)
        {
            return building.ValidateConditions(layer.Condition, layer.ModDataFlags);
        }

        private static IEnumerable<CodeInstruction> DrawTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                int insertIndex = -1;
                object drawLayer = null;
                object continueLabel = null;

                // Get the indices to insert at
                var lines = instructions.ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].opcode == OpCodes.Brtrue && lines[i - 1].opcode == OpCodes.Ldfld && lines[i - 1].operand.ToString().Contains("DrawInBackground", StringComparison.OrdinalIgnoreCase))
                    {
                        drawLayer = lines[i - 2].operand;
                        continueLabel = lines[i].operand;

                        insertIndex = i;
                        continue;
                    }
                }

                if (insertIndex == -1)
                {
                    throw new Exception("Unable to find insert position.");
                }

                // Insert the changes at the specified indices
                lines.Insert(insertIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                lines.Insert(insertIndex + 2, new CodeInstruction(OpCodes.Ldloc, drawLayer));
                lines.Insert(insertIndex + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingPatch), nameof(ValidateConditionsForDrawLayer))));
                lines.Insert(insertIndex + 4, new CodeInstruction(OpCodes.Brtrue_S, continueLabel));

                return lines;
            }
            catch (Exception e)
            {
                _monitor.Log($"There was an issue modifying the instructions for StardewValley.Buildings.Building.draw: {e}", LogLevel.Error);
                return instructions;
            }
        }        

        private static IEnumerable<CodeInstruction> DrawBackgroundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                int insertIndex = -1;
                object drawLayer = null;
                object continueLabel = null;

                // Get the indices to insert at
                var lines = instructions.ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].opcode == OpCodes.Brfalse && lines[i - 1].opcode == OpCodes.Ldfld && lines[i - 1].operand.ToString().Contains("DrawInBackground", StringComparison.OrdinalIgnoreCase))
                    {
                        drawLayer = lines[i - 2].operand;
                        continueLabel = lines[i].operand;

                        insertIndex = i;
                        continue;
                    }
                }

                if (insertIndex == -1)
                {
                    throw new Exception("Unable to find insert position.");
                }

                // Insert the changes at the specified indices
                lines.Insert(insertIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                lines.Insert(insertIndex + 2, new CodeInstruction(OpCodes.Ldloc, drawLayer));
                lines.Insert(insertIndex + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingPatch), nameof(ValidateConditionsForDrawLayer))));
                lines.Insert(insertIndex + 4, new CodeInstruction(OpCodes.Brtrue_S, continueLabel));

                return lines;
            }
            catch (Exception e)
            {
                _monitor.Log($"There was an issue modifying the instructions for StardewValley.Buildings.Building.drawInBackground: {e}", LogLevel.Error);
                return instructions;
            }
        }
        
        private static IEnumerable<CodeInstruction> DrawMenuTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                int insertIndex = -1;
                object drawLayer = null;
                object continueLabel = null;

                // Get the indices to insert at
                var lines = instructions.ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].opcode == OpCodes.Brfalse_S && lines[i - 1].opcode == OpCodes.Ldfld && lines[i - 1].operand.ToString().Contains("DrawInBackground", StringComparison.OrdinalIgnoreCase))
                    {
                        drawLayer = lines[i - 2].operand;
                        continueLabel = lines[i].operand;

                        insertIndex = i;
                        continue;
                    }
                }

                if (insertIndex == -1)
                {
                    throw new Exception("Unable to find insert position.");
                }

                // Insert the changes at the specified indices
                lines.Insert(insertIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                lines.Insert(insertIndex + 2, new CodeInstruction(OpCodes.Ldloc, drawLayer));
                lines.Insert(insertIndex + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingPatch), nameof(ValidateConditionsForDrawLayer))));
                lines.Insert(insertIndex + 4, new CodeInstruction(OpCodes.Brtrue_S, continueLabel));

                return lines;
            }
            catch (Exception e)
            {
                _monitor.Log($"There was an issue modifying the instructions for StardewValley.Buildings.Building.drawInBackground: {e}", LogLevel.Error);
                return instructions;
            }
        }
        
        private static IEnumerable<CodeInstruction> DrawInConstructionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                int insertIndex = -1;
                object drawLayer = null;
                object continueLabel = null;

                // Get the indices to insert at
                var lines = instructions.ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].opcode == OpCodes.Brtrue && lines[i - 1].opcode == OpCodes.Ldfld && lines[i - 1].operand.ToString().Contains("OnlyDrawIfChestHasContents", StringComparison.OrdinalIgnoreCase))
                    {
                        drawLayer = lines[i - 2].operand;
                        continueLabel = lines[i].operand;

                        insertIndex = i;
                        continue;
                    }
                }

                if (insertIndex == -1)
                {
                    throw new Exception("Unable to find insert position.");
                }

                // Insert the changes at the specified indices
                lines.Insert(insertIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                lines.Insert(insertIndex + 2, new CodeInstruction(OpCodes.Ldloc, drawLayer));
                lines.Insert(insertIndex + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingPatch), nameof(ValidateConditionsForDrawLayer))));
                lines.Insert(insertIndex + 4, new CodeInstruction(OpCodes.Brtrue_S, continueLabel));

                return lines;
            }
            catch (Exception e)
            {
                _monitor.Log($"There was an issue modifying the instructions for StardewValley.Buildings.Building.drawInBackground: {e}", LogLevel.Error);
                return instructions;
            }
        }

        private static void BuildingPostfix(Building __instance)
        {
            // Check if type is one of the extended models
            if (SolidFoundations.buildingManager.DoesBuildingModelExist(__instance.buildingType.Value) is false)
            {
                return;
            }
            var extendedModel = SolidFoundations.buildingManager.GetSpecificBuildingModel(__instance.buildingType.Value);

            // Update the chests to use the custom capacity
            foreach (ExtendedBuildingChest chestData in extendedModel.Chests)
            {
                if (__instance.GetBuildingChest(chestData.Name) is Chest chest && chest is not null)
                {
                    chest.modData[ModDataKeys.CUSTOM_CHEST_CAPACITY] = chestData.Capacity.ToString();
                }
            }
        }
    }
}
