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
    internal class BuildingPatch : PatchTemplate
    {
        private readonly Type _entity = typeof(Building);

        internal BuildingPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_entity, "getIndoors", new[] { typeof(string) }), prefix: new HarmonyMethod(GetType(), nameof(GetIndoorsPrefix)));
            harmony.Patch(AccessTools.Method(_entity, nameof(Building.resetTexture), null), prefix: new HarmonyMethod(GetType(), nameof(ResetTexturePrefix)));
        }

        private static bool GetIndoorsPrefix(Building __instance, string nameOfIndoorsWithoutUnique, ref GameLocation __result)
        {
            if (__instance is GenericBuilding genericBuilding)
            {
                __result = genericBuilding.GetIndoors();
                return false;
            }

            return true;
        }

        internal static bool ResetTexturePrefix(Building __instance)
        {
            if (__instance is not GenericBuilding genericBuilding)
            {
                return true;
            }

            var buildingModel = BetterBuildings.buildingManager.GetSpecificBuildingModel<BuildingModel>(genericBuilding.Id);
            if (buildingModel is null || buildingModel.Texture is null)
            {
                return false;
            }

            __instance.texture = new Lazy<Texture2D>(delegate
            {
                return buildingModel.Texture;
            });

            return false;
        }
    }
}
