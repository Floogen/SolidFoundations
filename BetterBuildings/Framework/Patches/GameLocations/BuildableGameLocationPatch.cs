using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Patches.Buildings
{
    internal class BuildableGameLocationPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(BuildableGameLocation);

        internal BuildableGameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            //harmony.Patch(AccessTools.Method(_entity, nameof(BuildableGameLocation.isCollidingPosition), new[] { typeof(Microsoft.Xna.Framework.Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(IsCollidingPositionPostfix)));
        }

        private static void IsCollidingPositionPostfix(BuildableGameLocation __instance, ref bool __result, Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
        {
            if (__result && isFarmer)
            {
                foreach (GenericBuilding customBuilding in __instance.buildings.Where(b => b is GenericBuilding genericBuilding && genericBuilding.Model is not null))
                {
                    if (customBuilding.Model.WalkableTiles.Count <= 0)
                    {
                        continue;
                    }

                    foreach (var walkableTile in customBuilding.Model.WalkableTiles)
                    {
                        foreach (var tileLocation in walkableTile.GetActualTiles())
                        {
                            var tileRectangle = new Rectangle((tileLocation.X + customBuilding.tileX.Value) * 64, (tileLocation.Y + customBuilding.tileY.Value) * 64, 64, 64);
                            tileRectangle.Height += 64;
                            _monitor.Log($"{tileRectangle.X}, {tileRectangle.Y} | {position.X}, {position.Y}", LogLevel.Debug);
                            if (tileRectangle.Contains(position))
                            {
                                __result = false;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
