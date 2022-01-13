using BetterBuildings.Framework.Managers;
using BetterBuildings.Framework.Models.Buildings;
using BetterBuildings.Framework.Patches.Buildings;
using BetterBuildings.Framework.Patches.Menus;
using BetterBuildings.Framework.Patches.Outliers;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.IO;
using System.Linq;

namespace BetterBuildings
{
    internal class BetterBuildings : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static BuildingManager buildingManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // TODO: Implement the following building models: Functional (obelisk-like), Production (mill-like) and Enterable / Decoratable (shed-like)
            // TODO: For functional, make API to allow hooking into functional buildings to allow for C# usage

            // Load managers
            buildingManager = new BuildingManager(monitor, modHelper);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply building patches
                new BuildingPatch(monitor, helper).Apply(harmony);

                // Apply menu patches
                new CarpenterMenuPatch(monitor, helper).Apply(harmony);

                // Apply outlier patches
                new BluePrintPatch(monitor, helper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Load any owned content packs
            LoadContentPacks();
        }

        internal void LoadContentPacks(bool silent = false)
        {
            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

                try
                {
                    var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Buildings"));
                    if (!directoryPath.Exists)
                    {
                        Monitor.Log($"No Buildings folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                        return;
                    }

                    var buildingsFolder = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                    if (buildingsFolder.Count() == 0)
                    {
                        Monitor.Log($"No sub-folders found under Buildings for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                        return;
                    }

                    // Load in the buildings
                    foreach (var folder in buildingsFolder)
                    {
                        if (!File.Exists(Path.Combine(folder.FullName, "building.json")))
                        {
                            if (folder.GetDirectories().Count() == 0)
                            {
                                Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a building.json under {folder.Name}", LogLevel.Warn);
                            }

                            continue;
                        }

                        var parentFolderName = folder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                        var modelPath = Path.Combine(parentFolderName, folder.Name, "building.json");

                        // Parse the model and assign it the content pack's owner
                        GenericBuilding buildingModel = contentPack.ReadJsonFile<GenericBuilding>(modelPath);

                        // Verify the required Name property is set
                        if (String.IsNullOrEmpty(buildingModel.Name))
                        {
                            Monitor.Log($"Unable to add building from {buildingModel.Owner}: Missing the Name property", LogLevel.Warn);
                            continue;
                        }

                        // Set the PackName and Id
                        buildingModel.PackName = contentPack.Manifest.Name;
                        buildingModel.Owner = contentPack.Manifest.UniqueID;
                        buildingModel.Id = String.Concat(buildingModel.Owner, "/", "/", buildingModel.Name);

                        // Verify that a building with the name doesn't exist in this pack
                        if (buildingManager.GetSpecificBuildingModel<GenericBuilding>(buildingModel.Id) != null)
                        {
                            Monitor.Log($"Unable to add building from {contentPack.Manifest.Name}: This pack already contains a building with the name of {buildingModel.Name}", LogLevel.Warn);
                            continue;
                        }

                        // Verify that a blueprint is given
                        if (buildingModel.Blueprint is null)
                        {
                            Monitor.Log($"Unable to add building from {contentPack.Manifest.Name}: The building {buildingModel.Name} is missing its Blueprint property", LogLevel.Warn);
                            continue;
                        }

                        // Verify we are given a texture and if so, track it
                        if (!File.Exists(Path.Combine(folder.FullName, "building.png")))
                        {
                            Monitor.Log($"Unable to add building for {buildingModel.Name} from {contentPack.Manifest.Name}: No associated building.png given", LogLevel.Warn);
                            continue;
                        }

                        // Load in the texture
                        buildingModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, folder.Name, "building.png")));

                        // Setup the blueprint
                        buildingModel.EstablishBlueprint();

                        // Track the model
                        buildingManager.AddBuilding(buildingModel);

                        // Log it
                        Monitor.Log(buildingModel.ToString(), LogLevel.Trace);
                    }

                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error loading buildings from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
                }
            }
        }
    }
}
