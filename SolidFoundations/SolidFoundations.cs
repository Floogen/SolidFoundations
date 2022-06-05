using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Interfaces.Internal;
using SolidFoundations.Framework.Managers;
using SolidFoundations.Framework.Models.Buildings;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using SolidFoundations.Framework.Patches.Buildings;
using SolidFoundations.Framework.Utilities;
using SolidFoundations.Framework.Utilities.Backport;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using xTile;
using xTile.Tiles;

namespace SolidFoundations
{
    public class SolidFoundations : Mod
    {
        // Shared static helpers
        internal static Api api;
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        // Managers
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;
        internal static BuildingManager buildingManager;

        public override void Entry(IModHelper helper)
        {
            // Validate that the game version is compatible
            if (IsGameVersionCompatible() is false)
            {
                Monitor.Log($"This version of Solid Foundations (v{ModManifest.Version}) is not compatible with Stardew Valley v{Game1.version}.\nSolid Foundations buildings will not be loaded.\nDownload the latest version of Solid Foundations to resolve this issue.", LogLevel.Error);
                return;
            }

            // Set up the monitor, helper and multiplayer
            api = new Api();
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Set up the managers
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(monitor, helper);
            buildingManager = new BuildingManager(monitor, helper);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply location patches
                new GameLocationPatch(monitor, helper).Apply(harmony);

                // Apply building patches
                new BluePrintPatch(monitor, helper).Apply(harmony);
                new BuildingPatch(monitor, helper).Apply(harmony);

                // Apply menu patches
                new CarpenterMenuPatch(monitor, helper).Apply(harmony);
                new PurchaseAnimalsMenuPatch(monitor, helper).Apply(harmony);

                // Apply etc. patches
                new QuestionEventPatch(monitor, helper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in the debug commands
            helper.ConsoleCommands.Add("sf_reload", "Reloads all Solid Foundations content packs.\n\nUsage: sf_reload", delegate { this.LoadContentPacks(); this.RefreshAllCustomBuildings(); });

            // Hook into the required events
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
            modHelper.Events.Content.AssetRequested += OnAssetRequested;
            modHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
            modHelper.Events.GameLoop.DayStarted += OnDayStarted;
            modHelper.Events.GameLoop.DayEnding += OnDayEnding;

            modHelper.Events.World.BuildingListChanged += OnBuildingListChanged;
        }

        // TODO: Remove this once this framework has been updated for SDV v1.6
        private bool IsGameVersionCompatible()
        {
            var incompatibleVersion = new Version("1.6.0");
            var gameVersion = new Version(Game1.version);

            return incompatibleVersion > gameVersion;
        }

        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            foreach (GenericBuilding building in e.Added.Where(b => b is GenericBuilding))
            {
                RefreshCustomBuilding(e.Location, building, true);
            }
        }

        // TODO: When using SDV v1.6, delete this event hook (will preserve modData flag removal)
        [EventPriority(EventPriority.High + 1)]
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            SafelyCacheCustomBuildings();
        }

        // TODO: When using SDV v1.6, repurpose this to convert all GenericBuildings into SDV Buildings
        [EventPriority(EventPriority.High + 1)]
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            LoadCachedCustomBuildings();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("Cherry.ShopTileFramework") && apiManager.HookIntoShopTileFramework(Helper))
            {
                // Do nothing
            }
            if (Helper.ModRegistry.IsLoaded("Omegasis.SaveAnywhere") && apiManager.HookIntoSaveAnywhere(Helper))
            {
                var saveAnywhereApi = apiManager.GetSaveAnywhereApi();

                // Hook into save related events
                saveAnywhereApi.BeforeSave += delegate { SafelyCacheCustomBuildings(); };
                saveAnywhereApi.AfterLoad += delegate { LoadCachedCustomBuildings(); };
            }

            // Load any owned content packs
            LoadContentPacks();

            // Set up the backported GameStateQuery
            GameStateQuery.SetupQueryTypes();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BuildingsData"))
            {
                e.LoadFrom(() => buildingManager.GetIdToModels(), AssetLoadPriority.Exclusive);
            }
            else if (e.DataType == typeof(Texture2D))
            {
                var asset = e.Name;
                if (buildingManager.GetTextureAsset(asset.Name) is var texturePath && texturePath is not null)
                {
                    e.LoadFrom(() => Helper.ModContent.Load<Texture2D>(texturePath), AssetLoadPriority.Exclusive);
                }
                else if (buildingManager.GetTileSheetAsset(asset.Name) is var tileSheetPath && tileSheetPath is not null)
                {
                    e.LoadFrom(() => Helper.ModContent.Load<Texture2D>(tileSheetPath), AssetLoadPriority.Exclusive);
                }
            }
            else if (e.DataType == typeof(Map))
            {
                var asset = e.Name;
                if (buildingManager.GetMapAsset(asset.Name) is var mapPath && mapPath is not null)
                {
                    e.LoadFrom(() => Helper.ModContent.Load<Map>(mapPath), AssetLoadPriority.Exclusive);
                }
            }
        }

        private void OnAssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            var asset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/BuildingsData"));
            if (asset is null)
            {
                return;
            }

            _ = Helper.GameContent.Load<Dictionary<string, ExtendedBuildingModel>>(asset);
        }

        public override object GetApi()
        {
            return api;
        }

        private void SafelyCacheCustomBuildings()
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }

            try
            {
                // Create the SolidFoundations folder near the save file, if one doesn't exist
                var externalSaveFolderPath = Path.Combine(Constants.CurrentSavePath, "SolidFoundations");
                if (!Directory.Exists(externalSaveFolderPath))
                {
                    Directory.CreateDirectory(externalSaveFolderPath);
                }

                // Process each buildable location and archive the relevant data
                var allExistingCustomBuildings = new List<GenericBuilding>();
                foreach (BuildableGameLocation buildableLocation in Game1.locations.Where(l => l is BuildableGameLocation buildableLocation && buildableLocation is not null && buildableLocation.buildings is not null))
                {
                    var archivedBuildingsData = new List<ArchivedBuildingData>();
                    foreach (GenericBuilding customBuilding in buildableLocation.buildings.Where(b => b is GenericBuilding).ToList())
                    {
                        // Remove any FlagType.Temporary stored in the buildings modData
                        foreach (var key in customBuilding.modData.Keys.Where(k => k.Contains(ModDataKeys.FLAG_BASE)).ToList())
                        {
                            if (customBuilding.modData[key] == SpecialAction.FlagType.Temporary.ToString())
                            {
                                customBuilding.modData.Remove(key);
                            }
                        }

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
            catch (Exception ex)
            {
                Monitor.Log("Failed to cache the custom buildings: Any changes made in the last game day will be lost to allow saving!", LogLevel.Warn);
                Monitor.Log($"Failure to cache the custom buildings: {ex}", LogLevel.Trace);

                foreach (BuildableGameLocation buildableLocation in Game1.locations.Where(l => l is BuildableGameLocation buildableLocation && buildableLocation is not null && buildableLocation.buildings is not null))
                {
                    foreach (GenericBuilding customBuilding in buildableLocation.buildings.Where(b => b is GenericBuilding).ToList())
                    {
                        try
                        {
                            // Remove the building from the location to avoid serialization issues
                            buildableLocation.buildings.Remove(customBuilding);
                        }
                        catch (Exception subEx)
                        {
                            var buildingName = customBuilding is null || customBuilding.Model is null ? "Unknown" : customBuilding.Model.Name;
                            Monitor.Log($"Failed to delete the custom building {buildingName}: Saving may fail!", LogLevel.Warn);
                            Monitor.Log($"Failed to delete the custom building {buildingName}: {subEx}", LogLevel.Trace);
                        }
                    }
                }
            }
        }

        private void LoadCachedCustomBuildings()
        {
            if (!Game1.IsMasterGame || String.IsNullOrEmpty(Constants.CurrentSavePath) || !File.Exists(Path.Combine(Constants.CurrentSavePath, "SolidFoundations", "buildings.json")))
            {
                this.RefreshAllCustomBuildings();
                return;
            }
            var customBuildingsExternalSavePath = Path.Combine(Constants.CurrentSavePath, "SolidFoundations", "buildings.json");

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
                    try
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
                        GameLocation interior = null;
                        if (customBuilding.indoors.Value is not null)
                        {
                            interior = customBuilding.indoors.Value;
                        }

                        // Update the building's model
                        customBuilding.RefreshModel(buildingManager.GetSpecificBuildingModel(customBuilding.Id));

                        // Load the building
                        customBuilding.load();

                        // Set the location
                        customBuilding.buildingLocation.Value = buildableLocation;

                        // Restore the archived custom building
                        buildableLocation.buildings.Add(customBuilding);

                        // Trigger the missed DayUpdate
                        customBuilding.dayUpdate(Game1.dayOfMonth);

                        // Clear any grass and other debris
                        var validIndexesForRemoval = new List<int>()
                        {
                            343,
                            450,
                            294,
                            295,
                            675,
                            674,
                            784,
                            677,
                            676,
                            785,
                            679,
                            678,
                            786,
                            674
                        };
                        for (int x = 0; x < customBuilding.tilesWide.Value; x++)
                        {
                            for (int y = 0; y < customBuilding.tilesHigh.Value; y++)
                            {
                                var targetTile = new Vector2(customBuilding.tileX.Value + x, customBuilding.tileY.Value + y);
                                if (buildableLocation.terrainFeatures.ContainsKey(targetTile) && buildableLocation.terrainFeatures[targetTile] is Grass grass && grass is not null)
                                {
                                    buildableLocation.terrainFeatures.Remove(targetTile);
                                }
                                else if (buildableLocation.terrainFeatures.ContainsKey(targetTile) && buildableLocation.terrainFeatures[targetTile] is Tree tree && tree is not null)
                                {
                                    buildableLocation.terrainFeatures.Remove(targetTile);
                                }
                                else if (buildableLocation.objects.ContainsKey(targetTile) && buildableLocation.objects[targetTile] is StardewValley.Object obj && obj is not null && validIndexesForRemoval.Contains(obj.ParentSheetIndex))
                                {
                                    buildableLocation.objects.Remove(targetTile);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Failed to load cached custom building {archivedData.Id} at [{archivedData.TileX}, {archivedData.TileY}], see log for details.", LogLevel.Warn);
                        Monitor.Log($"Failure to load the custom building: {ex}", LogLevel.Trace);
                    }
                }
            }
        }

        // TODO: When SDV v1.6 is released, revise this method to load the buildings into BuildingsData
        private void LoadContentPacks(bool silent = false)
        {
            // Clear the existing cache of custom buildings
            buildingManager.Reset();

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

                    // Load interiors
                    Monitor.Log($"Loading interiors from pack: {contentPack.Manifest.Name}", LogLevel.Trace);
                    LoadInteriors(contentPack);

                    // Load the buildings
                    Monitor.Log($"Loading buildings from pack: {contentPack.Manifest.Name}", LogLevel.Trace);
                    LoadBuildings(contentPack);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to load the content pack {contentPack.Manifest.UniqueID}: {ex}", LogLevel.Warn);
                }
            }

            // Load the buildings into the backported BuildingsData
            _ = Helper.GameContent.Load<Dictionary<string, ExtendedBuildingModel>>("Data/BuildingsData");
        }

        private void LoadInteriors(IContentPack contentPack)
        {
            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Interiors"));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No Interiors folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var interiorFiles = directoryPath.GetFiles("*.tmx", SearchOption.AllDirectories);
                if (interiorFiles.Count() == 0)
                {
                    Monitor.Log($"No TMX files found under Interiors for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                foreach (var interiorFile in interiorFiles)
                {
                    // Cache the interior map
                    if (interiorFile.Exists)
                    {
                        var mapPath = contentPack.ModContent.GetInternalAssetName(interiorFile.FullName).Name;
                        buildingManager.AddMapAsset(Path.GetFileNameWithoutExtension(interiorFile.Name), interiorFile.FullName);

                        Monitor.Log($"Loaded the interior {mapPath}", LogLevel.Trace);
                    }
                    else
                    {
                        Monitor.Log($"Unable to add interior for {interiorFile.FullName} from {contentPack.Manifest.Name}!", LogLevel.Warn);
                        continue;
                    }
                }

                var tilesheetFiles = directoryPath.GetFiles("*.png", SearchOption.AllDirectories);
                if (tilesheetFiles.Count() == 0)
                {
                    Monitor.Log($"No tilesheets found under Interiors for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                foreach (var tileSheetFile in tilesheetFiles)
                {
                    // Cache the interior map
                    if (tileSheetFile.Exists)
                    {
                        var tilesheetPath = contentPack.ModContent.GetInternalAssetName(tileSheetFile.FullName).Name;
                        var tilesheetPathWithoutExtension = tilesheetPath.Replace(".png", null);
                        buildingManager.AddTileSheetAsset(tilesheetPath, tileSheetFile.FullName);

                        if (tilesheetPath != tilesheetPathWithoutExtension)
                        {
                            buildingManager.AddTileSheetAsset(tilesheetPathWithoutExtension, tileSheetFile.FullName);
                        }

                        Monitor.Log($"Loaded the tilesheet {tilesheetPath} | {tilesheetPathWithoutExtension}", LogLevel.Trace);
                    }
                    else
                    {
                        Monitor.Log($"Unable to add tilesheet for {tileSheetFile.FullName} from {contentPack.Manifest.Name}!", LogLevel.Warn);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading interiors / tilesheets from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
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

                var buildingsFolder = directoryPath.GetDirectories("*", SearchOption.TopDirectoryOnly);
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

                    if (String.IsNullOrEmpty(buildingModel.ID))
                    {
                        buildingModel.ID = String.Concat(buildingModel.Owner, "_", buildingModel.Name.Replace(" ", String.Empty));
                    }

                    // Verify that a building with the name doesn't exist in this pack
                    if (buildingManager.GetSpecificBuildingModel(buildingModel.ID) != null)
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

                    // Load in any skins, if given
                    if (Directory.Exists(Path.Combine(folder.FullName, "Skins")))
                    {
                        List<string> skinPaths = Directory.GetFiles(Path.Combine(folder.FullName, "Skins"), "*.png").OrderBy(s => s).ToList();
                        foreach (var skin in buildingModel.Skins)
                        {
                            var skinPath = Path.Combine(folder.FullName, "Skins", String.Concat(skin.Texture, ".png"));
                            if (skinPaths.Contains(skinPath) is false)
                            {
                                continue;
                            }

                            skin.Texture = contentPack.ModContent.GetInternalAssetName(skinPath).Name;
                            buildingManager.AddTextureAsset(skin.Texture, skinPath);
                        }
                    }

                    // Load in the sprites for DrawLayers, if given
                    if (Directory.Exists(Path.Combine(folder.FullName, "Sprites")) && buildingModel.DrawLayers is not null)
                    {
                        List<string> spritePaths = Directory.GetFiles(Path.Combine(folder.FullName, "Sprites"), "*.png").ToList();
                        foreach (var layer in buildingModel.DrawLayers.Where(t => String.IsNullOrEmpty(t.Texture) is false))
                        {
                            var spritePath = Path.Combine(folder.FullName, "Sprites", String.Concat(layer.Texture, ".png"));
                            if (spritePaths.Contains(spritePath) is false)
                            {
                                Monitor.Log($"Unable to find the texture {spritePath} under Sprites from for {buildingModel.Name} from {contentPack.Manifest.Name}, assuming to be vanilla or a texture loaded via Content Patcher.", LogLevel.Trace);
                                continue;
                            }

                            layer.Texture = contentPack.ModContent.GetInternalAssetName(spritePath).Name;
                            buildingManager.AddTextureAsset(layer.Texture, spritePath);
                        }
                    }

                    // Load in the texture
                    var texturePath = Path.Combine(folder.FullName, "building.png");
                    buildingModel.Texture = contentPack.ModContent.GetInternalAssetName(texturePath).Name;
                    buildingManager.AddTextureAsset(buildingModel.Texture, texturePath);

                    // Track the model
                    buildingManager.AddBuilding(buildingModel);

                    // Log it
                    Monitor.Log($"Loaded the building {buildingModel.ID}", LogLevel.Trace);
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
                    RefreshCustomBuilding(buildableLocation, building, resetTexture);
                }
            }
        }

        private void RefreshCustomBuilding(GameLocation location, GenericBuilding building, bool resetTexture = true)
        {
            try
            {
                var model = buildingManager.GetSpecificBuildingModel(building.Id);
                if (model is not null)
                {
                    // Remove any FlagType.Temporary stored in the buildings modData
                    foreach (var key in building.modData.Keys.Where(k => k.Contains(ModDataKeys.FLAG_BASE)).ToList())
                    {
                        if (building.modData[key] == SpecialAction.FlagType.Temporary.ToString())
                        {
                            building.modData.Remove(key);
                        }
                    }

                    building.RefreshModel(model);

                    if (resetTexture)
                    {
                        Helper.GameContent.InvalidateCache(model.Texture);
                        building.resetTexture();
                    }
                }
                else
                {
                    throw new Exception("Model is null.");
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to refresh {building.Id} | {building.textureName()} from {location.NameOrUniqueName}!", LogLevel.Warn);
                Monitor.Log($"Failed to refresh {building.Id} | {building.textureName()} from {location.NameOrUniqueName}: {ex}", LogLevel.Trace);
            }
        }
    }
}
