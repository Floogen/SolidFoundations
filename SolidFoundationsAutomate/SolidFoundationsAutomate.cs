using SolidFoundationsAutomate.Framework.External.Automate;
using SolidFoundationsAutomate.Framework.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;

namespace SolidFoundationsAutomate
{
    public class SolidFoundationsAutomate : Mod
    {

        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static ApiManager apiManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Set up the managers
            apiManager = new ApiManager(monitor);

            // Hook into the required events
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("Pathoschild.Automate") && apiManager.HookIntoAutomate(Helper))
            {
                apiManager.GetAutomateApi().AddFactory(new BuildingFactory());
            }
        }
    }
}
