using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Managers;
using SolidFoundations.Framework.Models.Buildings;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Patches.Buildings;
using SolidFoundations.Framework.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;

namespace SolidFoundations
{
    public class SolidFoundations : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static BuildingManager buildingManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Set up the managers
            buildingManager = new BuildingManager(monitor, helper);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply building patches
                new BluePrintPatch(monitor, helper).Apply(harmony);
                new BuildingPatch(monitor, helper).Apply(harmony);

                // Apply menu patches
                new CarpenterMenuPatch(monitor, helper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in the debug commands
            helper.ConsoleCommands.Add("sf_reload", "Reloads all Solid Foundations content packs.\n\nUsage: sf_reload", delegate { this.LoadContentPacks(); this.RefreshAllCustomBuildings(); });

            // Hook into the required events
            modHelper.Events.Content.AssetRequested += OnAssetRequested;
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
            modHelper.Events.GameLoop.TimeChanged += OnTimeChanged;
            modHelper.Events.GameLoop.DayEnding += OnDayEnding;
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }

            // Create the SolidFoundations folder near the save file, if one doesn't exist
            var externalSaveFolderPath = Path.Combine(Constants.CurrentSavePath, "SolidFoundations");
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
                    archivedBuildingsData.Add(new ArchivedBuildingData() { Id = customBuilding.Id, TileX = customBuilding.tileX.Value, TileY = customBuilding.tileY.Value });

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

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // TODO: Implement this
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            var customBuildingsExternalSavePath = Path.Combine(Constants.CurrentSavePath, "SolidFoundations", "buildings.json");
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

                    GenericBuilding customBuilding = externallySavedCustomBuildings.FirstOrDefault(b => b.Id == archivedData.Id && b.tileX.Value == archivedData.TileX && b.tileY.Value == archivedData.TileY);
                    if (customBuilding is null)
                    {
                        continue;
                    }

                    // Update the building's model
                    customBuilding.RefreshModel(buildingManager.GetSpecificBuildingModel<ExtendedBuildingModel>(customBuilding.Id));

                    // Load the building
                    customBuilding.load();

                    // Restore the archived custom building
                    buildableLocation.buildings.Add(customBuilding);

                    // Trigger the missed DayUpdate
                    customBuilding.dayUpdate(Game1.dayOfMonth);
                }
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Load any owned content packs
            LoadContentPacks();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.DataType == typeof(Texture2D))
            {
                var asset = e.Name;
                if (buildingManager.GetTextureAsset(asset.Name) is var texturePath && texturePath is not null)
                {
                    e.LoadFrom(() => Helper.ModContent.Load<Texture2D>(texturePath), AssetLoadPriority.Exclusive);
                }
            }
            else if (e.DataType == typeof(string))
            {
                var asset = e.Name;
                if (buildingManager.GetMapAsset(asset.Name) is var mapPath && mapPath is not null)
                {
                    e.LoadFrom(() => mapPath, AssetLoadPriority.Exclusive);
                }
            }
        }

        private void LoadContentPacks(bool silent = false)
        {
            // Clear the existing cache of custom buildings
            buildingManager.Reset();

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

                // Load the buildings
                Monitor.Log($"Loading buildings from pack: {contentPack.Manifest.Name}", LogLevel.Trace);
                LoadBuildings(contentPack);
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
                    ExtendedBuildingModel buildingModel = contentPack.ReadJsonFile<ExtendedBuildingModel>(modelPath);

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(buildingModel.Name))
                    {
                        Monitor.Log($"Unable to add building from content pack {contentPack.Manifest.Name}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Set the PackName and Id
                    buildingModel.PackName = contentPack.Manifest.Name;
                    buildingModel.Owner = contentPack.Manifest.UniqueID;
                    buildingModel.ID = String.Concat(buildingModel.Owner, "_", buildingModel.Name);

                    // Verify that a building with the name doesn't exist in this pack
                    if (buildingManager.GetSpecificBuildingModel<ExtendedBuildingModel>(buildingModel.ID) != null)
                    {
                        Monitor.Log($"Unable to add building from {contentPack.Manifest.Name}: This pack already contains a building with the name of {buildingModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(folder.FullName, "building.png")))
                    {
                        Monitor.Log($"Unable to add building for {buildingModel.Name} from {contentPack.Manifest.Name}: No associated building.png given", LogLevel.Warn);
                        continue;
                    }

                    // Verify that we are given an interior.tmx if any InteractiveTiles
                    var mapPath = Path.Combine(folder.FullName, "interior.tmx");
                    if (String.IsNullOrEmpty(buildingModel.AnimalDoor) is false || (buildingModel.HumanDoor.X != -1 && buildingModel.HumanDoor.Y != -1))
                    {
                        // Cache the interior map
                        if (File.Exists(mapPath))
                        {
                            buildingModel.IndoorMap = contentPack.ModContent.GetInternalAssetName(mapPath).Name;
                            buildingManager.AddMapAsset(buildingModel.IndoorMap, mapPath);
                        }
                        else
                        {
                            Monitor.Log($"Unable to add building for {buildingModel.Name} from {contentPack.Manifest.Name}: The Doorways property was used but no interior.tmx was found", LogLevel.Warn);
                            continue;
                        }
                    }

                    // Load in the texture
                    var texturePath = Path.Combine(folder.FullName, "building.png");
                    buildingModel.Texture = contentPack.ModContent.GetInternalAssetName(texturePath).Name;
                    buildingManager.AddTextureAsset(buildingModel.Texture, texturePath);

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

        private void RefreshAllCustomBuildings(bool resetTexture = true)
        {
            foreach (BuildableGameLocation buildableLocation in Game1.locations.Where(l => l is BuildableGameLocation))
            {
                foreach (GenericBuilding building in buildableLocation.buildings.Where(b => b is GenericBuilding))
                {
                    var model = buildingManager.GetSpecificBuildingModel<ExtendedBuildingModel>(building.Id);
                    if (model is not null)
                    {
                        building.RefreshModel(model);
                    }

                    if (resetTexture)
                    {
                        Helper.GameContent.InvalidateCache(model.Texture);
                        building.resetTexture();
                    }
                }
            }
        }
    }
}
