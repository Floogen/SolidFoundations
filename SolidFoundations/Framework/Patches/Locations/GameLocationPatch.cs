using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Utilities.Backport;
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
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace SolidFoundations.Framework.Patches.Buildings
{
    // TODO: When updated to SDV v1.6, delete this patch
    internal class GameLocationPatch : PatchTemplate
    {

        private readonly Type _object = typeof(GameLocation);

        internal GameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.performAction), new[] { typeof(string), typeof(Farmer), typeof(Location) }), postfix: new HarmonyMethod(GetType(), nameof(PerformActionPostfix)));
        }

        internal static void PerformActionPostfix(GameLocation __instance, ref bool __result, string action, Farmer who, Location tileLocation)
        {
            if (__instance is BuildableGameLocation buildableLocation is false || buildableLocation is null || __result is true)
            {
                return;
            }

            if (action != null && who.IsLocalPlayer)
            {
                string[] array = action.Split(' ');
                switch (array[0])
                {
                    case "BuildingChest":
                        {
                            GenericBuilding buildingAt2 = buildableLocation.getBuildingAt(new Vector2(tileLocation.X, tileLocation.Y)) as GenericBuilding;
                            if (buildingAt2 != null)
                            {
                                buildingAt2.PerformBuildingChestAction(array[1], who);
                                __result = true;
                            }
                            break;
                        }
                    case "BuildingToggleAnimalDoor":
                        {
                            GenericBuilding buildingAt = buildableLocation.getBuildingAt(new Vector2(tileLocation.X, tileLocation.Y)) as GenericBuilding;
                            if (buildingAt != null)
                            {
                                if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
                                {
                                    buildingAt.ToggleAnimalDoor(who);
                                }
                                __result = true;
                            }
                            break;
                        }
                }
            }
        }
    }
}
