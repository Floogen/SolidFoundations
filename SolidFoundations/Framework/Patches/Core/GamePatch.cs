using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Utilities;
using SolidFoundations.Framework.Utilities.Backport;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Events;
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

namespace SolidFoundations.Framework.Patches.Core
{
    // TODO: When updated to SDV v1.6, delete this patch
    internal class GamePatch : PatchTemplate
    {

        private readonly Type _object = typeof(Game1);

        internal GamePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "UpdateLocations", new[] { typeof(GameTime) }), postfix: new HarmonyMethod(GetType(), nameof(UpdateLocationsPostfix)));
        }

        // TODO: Remove this once this framework has been updated for SDV v1.6
        internal static void UpdateLocationsPostfix(Game1 __instance, GameTime time)
        {
            if (Game1.menuUp && !Game1.IsMultiplayer)
            {
                return;
            }

            foreach (GameLocation location in Game1.locations)
            {
                if (location is not BuildableGameLocation buildableGameLocation)
                {
                    continue;
                }

                foreach (Building building2 in buildableGameLocation.buildings)
                {
                    GameLocation interior = building2.indoors.Value;
                    if (interior != null && Game1.locations.Contains(interior) is false)
                    {
                        RecursivelyHandleSubBuildableLocations(time, interior);
                    }
                }
            }
        }

        private static void RecursivelyHandleSubBuildableLocations(GameTime time, GameLocation gameLocation)
        {
            if (gameLocation is null || gameLocation is not BuildableGameLocation buildableGameLocation)
            {
                return;
            }

            foreach (Building building in buildableGameLocation.buildings)
            {
                GameLocation interior = building.indoors.Value;
                if (interior is not null && Game1.locations.Contains(interior) is false)
                {
                    if (interior.farmers.Any())
                    {
                        interior.UpdateWhenCurrentLocation(time);
                    }
                    interior.updateEvenIfFarmerIsntHere(time);

                    RecursivelyHandleSubBuildableLocations(time, interior);
                }
            }
        }
    }
}
