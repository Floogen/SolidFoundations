using BetterBuildings.Framework.Managers;
using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.Effects;
using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Patches.Buildings;
using BetterBuildings.Framework.Patches.Menus;
using BetterBuildings.Framework.Patches.Outliers;
using BetterBuildings.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;

namespace BetterBuildings
{
    internal class BetterBuildings : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static ApiManager apiManager;
        internal static BuildingManager buildingManager;
        internal static EffectManager effectManager;

        // Flags
        internal static bool showWalkableTiles;
        internal static bool showBuildingTiles;
        internal static bool showInteractiveTiles;
        internal static bool showFadeBox;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // TODO: Implement the following building models: Functional (obelisk-like), Production (mill-like) and Enterable / Decoratable (shed-like)
            // TODO: For functional, make API to allow hooking into functional buildings to allow for C# usage

            // Load managers
            apiManager = new ApiManager(monitor);
            buildingManager = new BuildingManager(monitor, modHelper);
            effectManager = new EffectManager(monitor, modHelper);

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

            // Add in the debug commands
            helper.ConsoleCommands.Add("bb_reload", "Reloads all Better Buildings content packs.\n\nUsage: bb_reload", delegate { this.LoadContentPacks(); RefreshAllCustomBuildings(); });
            helper.ConsoleCommands.Add("bb_debug", "Draws all debug tiles for all custom buildings.\n\nUsage: bb_debug", delegate { showWalkableTiles = !showWalkableTiles; showBuildingTiles = !showBuildingTiles; showFadeBox = !showFadeBox; });
            helper.ConsoleCommands.Add("bb_show_walkable", "Draws all WalkableTiles for all custom buildings.\n\nUsage: bb_show_walkable", delegate { showWalkableTiles = !showWalkableTiles; });
            helper.ConsoleCommands.Add("bb_show_blocking", "Draws all WalkableTiles for all custom buildings.\n\nUsage: bb_show_blocking", delegate { showBuildingTiles = !showBuildingTiles; });
            helper.ConsoleCommands.Add("bb_show_interactive", "Draws all InteractiveTiles for all custom buildings.\n\nUsage: bb_show_interactive", delegate { showInteractiveTiles = !showInteractiveTiles; });
            helper.ConsoleCommands.Add("bb_show_fade_box", "Draws all WalkableTiles for all custom buildings.\n\nUsage: bb_show_fade_box", delegate { showFadeBox = !showFadeBox; });

            // Hook into the required events
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
            modHelper.Events.GameLoop.DayEnding += OnDayEnding;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            var customBuildingsExternalSavePath = Path.Combine(Constants.CurrentSavePath, "BetterBuildings", "buildings.json");
            if (!Game1.IsMasterGame || !File.Exists(customBuildingsExternalSavePath))
            {
                return;
            }

            // Get the externally saved custom building objects
            var externallySavedCustomBuildings = new List<GenericBuilding>();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<GenericBuilding>));
            using (StreamReader textReader = new StreamReader(customBuildingsExternalSavePath))
            {
                externallySavedCustomBuildings = (List<GenericBuilding>)xmlSerializer.Deserialize(textReader);
            }

            // Process each buildable location and restore any installed custom buildings
            foreach (BuildableGameLocation buildableLocation in Game1.locations.Where(l => l is BuildableGameLocation && l.modData.ContainsKey(ModDataKeys.LOCATION_CUSTOM_BUILDINGS)))
            {
                // Get the archived custom building data for this location
                var archivedBuildingsData = JsonSerializer.Deserialize<List<ArchivedBuildingData>>(buildableLocation.modData[ModDataKeys.LOCATION_CUSTOM_BUILDINGS]);

                // Go through each ArchivedBuildingData to confirm that a) the Id exists via BuildingManager and b) there is a match in locationNamesToCustomBuildings to its Id and TileLocation
                foreach (var archivedData in archivedBuildingsData)
                {
                    if (!buildingManager.DoesBuildingModelExist(archivedData.Id) || !externallySavedCustomBuildings.Any(b => b.LocationName == buildableLocation.NameOrUniqueName))
                    {
                        continue;
                    }

                    GenericBuilding customBuilding = externallySavedCustomBuildings.FirstOrDefault(b => b.Id == archivedData.Id && b.TileLocation.Equals(archivedData.TileLocation));
                    if (customBuilding is null)
                    {
                        continue;
                    }

                    // Update the building's model
                    customBuilding.RefreshModel(buildingManager.GetSpecificBuildingModel<BuildingModel>(customBuilding.Id));

                    // Load the building
                    customBuilding.load();

                    // Restore the archived custom building
                    buildableLocation.buildings.Add(customBuilding);

                    // Trigger the missed DayUpdate
                    customBuilding.dayUpdate(Game1.dayOfMonth);
                }
            }
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }

            // Create the BetterBuildings folder near the save file, if one doesn't exist
            var externalSaveFolderPath = Path.Combine(Constants.CurrentSavePath, "BetterBuildings");
            if (!Directory.Exists(externalSaveFolderPath))
            {
                Directory.CreateDirectory(externalSaveFolderPath);
            }

            // Process each buildable location and archive the relevant data
            var allExistingCustomBuildings = new List<GenericBuilding>();
            foreach (BuildableGameLocation buildableLocation in Game1.locations.Where(l => l is BuildableGameLocation))
            {
                var archivedBuildingsData = new List<ArchivedBuildingData>();
                foreach (GenericBuilding customBuilding in buildableLocation.buildings.Where(b => b is GenericBuilding).ToList())
                {
                    // Prepare the custom building objects for this location to be stored externally
                    allExistingCustomBuildings.Add(customBuilding);
                    archivedBuildingsData.Add(new ArchivedBuildingData() { Id = customBuilding.Id, TileLocation = customBuilding.TileLocation });

                    // Remove the building from the location to avoid serialization issues
                    buildableLocation.buildings.Remove(customBuilding);
                }

                // Archive the custom building data for this location
                buildableLocation.modData[ModDataKeys.LOCATION_CUSTOM_BUILDINGS] = JsonSerializer.Serialize(archivedBuildingsData);
            }

            // Save the custom building objects externally, at the player's save file location
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<GenericBuilding>));
            using (StreamWriter writer = new StreamWriter(Path.Combine(externalSaveFolderPath, "buildings.json")))
            {
                xmlSerializer.Serialize(writer, allExistingCustomBuildings);
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && apiManager.HookIntoContentPatcher(Helper))
            {
                //apiManager.GetContentPatcherApi().RegisterToken(ModManifest, "Building", new BuildingToken());
            }
            if (Helper.ModRegistry.IsLoaded("Cherry.ShopTileFramework") && apiManager.HookIntoShopTileFramework(Helper))
            {
                // Do nothing
            }

            // Load any owned content packs
            LoadContentPacks();
        }

        internal void LoadContentPacks(bool silent = false)
        {
            // Clear the existing cache of custom buildings
            buildingManager.Reset();

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

                // Load the effects
                Monitor.Log($"Loading effects from pack: {contentPack.Manifest.Name}", LogLevel.Trace);
                LoadEffects(contentPack);

                // Load the buildings
                Monitor.Log($"Loading buildings from pack: {contentPack.Manifest.Name}", LogLevel.Trace);
                LoadBuildings(contentPack);
            }
        }

        private void LoadEffects(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Effects"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Effects folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var buildingsFolder = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (buildingsFolder.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Effects for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the buildings
                foreach (var folder in buildingsFolder)
                {
                    if (!File.Exists(Path.Combine(folder.FullName, "effect.json")))
                    {
                        if (folder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a effect.json under {folder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = folder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, folder.Name, "effect.json");

                    // Parse the model and assign it the content pack's owner
                    EffectModel effectModel = contentPack.ReadJsonFile<EffectModel>(modelPath);

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(effectModel.Name))
                    {
                        Monitor.Log($"Unable to add effect from {effectModel.Owner}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the PackName and Id
                    effectModel.PackName = contentPack.Manifest.Name;
                    effectModel.Owner = contentPack.Manifest.UniqueID;
                    effectModel.Id = String.Concat(effectModel.Owner, "/", "/", effectModel.Name);

                    // Verify that a building with the name doesn't exist in this pack
                    if (buildingManager.GetSpecificBuildingModel<BuildingModel>(effectModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add effect from {contentPack.Manifest.Name}: This pack already contains an effect with the name of {effectModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(folder.FullName, "effect.png")))
                    {
                        Monitor.Log($"Unable to add effect for {effectModel.Name} from {contentPack.Manifest.Name}: No associated effect.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    effectModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, folder.Name, "effect.png")));

                    // Track the model
                    effectManager.AddEffect(effectModel);

                    // Log it
                    Monitor.Log(effectModel.ToString(), LogLevel.Trace);
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading buildings from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        private void LoadBuildings(IContentPack contentPack)
        {
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
                    BuildingModel buildingModel = contentPack.ReadJsonFile<BuildingModel>(modelPath);

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
                    if (buildingManager.GetSpecificBuildingModel<BuildingModel>(buildingModel.Id) != null)
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

                    // Verify that we are given an interior.tmx if any InteractiveTiles
                    if (buildingModel.Doorways is not null && !File.Exists(Path.Combine(folder.FullName, "interior.tmx")))
                    {
                        Monitor.Log($"Unable to add building for {buildingModel.Name} from {contentPack.Manifest.Name}: The Doorways property was used but no interior.tmx was found", LogLevel.Warn);
                        continue;
                    }

                    // Cache the interior map
                    if (File.Exists(Path.Combine(folder.FullName, "interior.tmx")))
                    {
                        buildingModel.MapPath = contentPack.GetActualAssetKey(Path.Combine(folder.Parent.Name, folder.Name, "interior.tmx"));
                    }

                    // Load in the texture
                    buildingModel.Texture = contentPack.LoadAsset<Texture2D>(contentPack.GetActualAssetKey(Path.Combine(parentFolderName, folder.Name, "building.png")));

                    // Setup the blueprint
                    buildingModel.EstablishBlueprint();

                    // Load in any effects
                    foreach (GenericEffect effectData in buildingModel.Effects.ToList())
                    {
                        var effectModel = effectManager.GetSpecificEffectModel<EffectModel>(String.Concat(String.Concat(buildingModel.Owner, "/", "/", effectData.Name)));
                        if (effectModel is null)
                        {
                            Monitor.Log($"The building {buildingModel.Name} attempted to load an effect ({effectData.Name}) that doesn't exist under the Effects folder from {contentPack.Manifest.Name}", LogLevel.Warn);
                            buildingModel.Effects.Remove(effectData);

                            continue;
                        }

                        effectData.Model = effectModel;
                    }

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

        internal void RefreshAllCustomBuildings(bool resetTexture = true)
        {
            foreach (BuildableGameLocation buildableLocation in Game1.locations.Where(l => l is BuildableGameLocation))
            {
                foreach (GenericBuilding building in buildableLocation.buildings.Where(b => b is GenericBuilding))
                {
                    var model = buildingManager.GetSpecificBuildingModel<BuildingModel>(building.Id);
                    if (model is not null)
                    {
                        building.RefreshModel(model);
                    }

                    if (resetTexture)
                    {
                        building.resetTexture();
                    }
                }
            }
        }
    }
}
