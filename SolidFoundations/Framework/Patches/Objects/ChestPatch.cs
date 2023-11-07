using HarmonyLib;
using SolidFoundations.Framework.Utilities;
using StardewModdingAPI;
using StardewValley.Objects;
using System;

namespace SolidFoundations.Framework.Patches.Buildings
{
    internal class ChestPatch : PatchTemplate
    {

        private readonly Type _object = typeof(Chest);

        internal ChestPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Chest.GetActualCapacity), null), postfix: new HarmonyMethod(GetType(), nameof(GetActualCapacityPostfix)));
        }

        internal static void GetActualCapacityPostfix(Chest __instance, ref int __result)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.CUSTOM_CHEST_CAPACITY) && int.TryParse(__instance.modData[ModDataKeys.CUSTOM_CHEST_CAPACITY], out int capacity))
            {
                __result = capacity;
            }
        }
    }
}
