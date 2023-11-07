using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFoundations.Framework.Extensions;
using SolidFoundations.Framework.External.ContentPatcher;
using SolidFoundations.Framework.External.SpaceCore;
using SolidFoundations.Framework.Interfaces.Internal;
using SolidFoundations.Framework.Managers;
using SolidFoundations.Framework.Models.Buildings;
using SolidFoundations.Framework.Models.ContentPack;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using SolidFoundations.Framework.Models.ContentPack.Compatibility;
using SolidFoundations.Framework.Patches.Buildings;
using SolidFoundations.Framework.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
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

                // Apply building patches
                new BuildingPatch(monitor, helper).Apply(harmony);

                // Apply object patch
                new ChestPatch(monitor, helper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in the debug commands
            helper.ConsoleCommands.Add("sf_reload", "Reloads all Solid Foundations content packs.\n\nUsage: sf_reload", delegate { this.LoadContentPacks(); this.RefreshAllCustomBuildings(); });
            helper.ConsoleCommands.Add("sf_place_building", "Adds a building in the current location at given tile.\n\nUsage: sf_place_building MODEL_ID TILE_X TILE_Y", this.PlaceBuildingAtTile);

            // Hook into the required events
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
            helper.Events.Content.AssetRequested += OnAssetRequested;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            helper.Events.World.BuildingListChanged += OnBuildingListChanged;
        }

        private bool IsGameVersionCompatible()
        {
            var requiredMinimumVersion = new Version("1.6.0");
            var gameVersion = new Version(Game1.version);

            return gameVersion >= requiredMinimumVersion;
        }

        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            foreach (GenericBuilding building in e.Added.Where(b => b is GenericBuilding))
            {
                RefreshCustomBuilding(e.Location, building, true);
            }
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            // Load any owned content packs
            LoadContentPacks();

            // Invalidate the BuildingsData cache to reapply any patches
            //modHelper.GameContent.InvalidateCache("Data/Buildings");
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
            if (Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && apiManager.HookIntoContentPatcher(Helper))
            {
                apiManager.GetContentPatcherApi().RegisterToken(ModManifest, "IsBuildingHere", new IsBuildingHereToken());
                apiManager.GetContentPatcherApi().RegisterToken(ModManifest, "BuildingTexture", new BuildingTextureToken());
            }
            if (Helper.ModRegistry.IsLoaded("Cherry.ShopTileFramework") && apiManager.HookIntoShopTileFramework(Helper))
            {
                // Do nothing
            }
            if (Helper.ModRegistry.IsLoaded("Omegasis.SaveAnywhere") && apiManager.HookIntoSaveAnywhere(Helper))
            {
                var saveAnywhereApi = apiManager.GetSaveAnywhereApi();

                // Hook into save related events
                saveAnywhereApi.AfterLoad += delegate { LoadCachedCustomBuildings(); };
            }

            // Load any owned content packs
            LoadContentPacks();

            // Register our game state queries
            GameStateQueries.Register();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    foreach (var idToModel in buildingManager.GetIdToModels())
                    {
                        asset.AsDictionary<string, BuildingData>().Data[idToModel.Key] = idToModel.Value;
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/PaintData"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    // Add any building masks
                    var models = buildingManager.GetAllBuildingModels();
                    foreach (var model in models.Where(m => m.PaintMasks is not null))
                    {
                        string parsedMaskText = null;
                        foreach (var mask in model.PaintMasks)
                        {
                            parsedMaskText += $"{mask.Name}/{mask.MinBrightness} {mask.MaxBrightness}/";
                        }

                        if (String.IsNullOrEmpty(parsedMaskText) is false)
                        {
                            data[model.ID] = parsedMaskText;
                        }
                    }

                    // Add any building skin masks
                    foreach (var skin in models.Where(m => m.Skins is not null).SelectMany(m => m.Skins))
                    {
                        if (skin.PaintMasks is null)
                        {
                            continue;
                        }

                        string parsedMaskText = null;
                        foreach (var mask in skin.PaintMasks)
                        {
                            parsedMaskText += $"{mask.Name}/{mask.MinBrightness} {mask.MaxBrightness}/";
                        }

                        if (String.IsNullOrEmpty(parsedMaskText) is false)
                        {
                            data[skin.Id] = parsedMaskText;
                        }
                    }
                });
            }
            else if (e.DataType == typeof(Texture2D))
            {
                var asset = e.Name;
                if (buildingManager.GetTextureAsset(asset.Name) is var texturePath && texturePath is not null)
                {
                    e.LoadFrom(() => Game1.content.Load<Texture2D>(texturePath), AssetLoadPriority.Exclusive);
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
                    e.LoadFrom(() => Game1.content.Load<Map>(mapPath), AssetLoadPriority.Exclusive);
                }
            }
        }

        private void OnAssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            var asset = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/Buildings"));
            if (asset is null)
            {
                return;
            }

            // Force load the changes
            var idToModels = Helper.GameContent.Load<Dictionary<string, BuildingData>>(asset);

            // Correct the DrawLayers.Texture and Skins.Texture ids
            foreach (BuildingData model in idToModels.Values)
            {
                var actualModel = model as ExtendedBuildingModel;
                if (actualModel is null)
                {
                    continue;
                }

                if (model.DrawLayers is not null)
                {
                    foreach (var layer in model.DrawLayers.Where(t => String.IsNullOrEmpty(t.Texture) is false))
                    {
                        var spriteAsset = $"{actualModel.ID}_Sprites_{Path.GetFileNameWithoutExtension(layer.Texture)}";
                        if (buildingManager.GetTextureAsset(spriteAsset) is not null)
                        {
                            layer.Texture = spriteAsset;
                        }
                    }
                }

                if (model.Skins is not null)
                {
                    foreach (var skin in model.Skins.Where(t => String.IsNullOrEmpty(t.Texture) is false))
                    {
                        var skinAsset = $"{actualModel.ID}_Skins_{Path.GetFileNameWithoutExtension(skin.Texture)}";
                        if (buildingManager.GetTextureAsset(skinAsset) is not null)
                        {
                            skin.Texture = skinAsset;
                        }
                    }
                }
            }
        }

        public override object GetApi()
        {
            return api;
        }

        private void LoadCachedCustomBuildings()
        {
            if (!Game1.IsMasterGame || String.IsNullOrEmpty(Constants.CurrentSavePath) || !File.Exists(Path.Combine(Constants.CurrentSavePath, "SolidFoundations", "buildings.json")))
            {
                this.RefreshAllCustomBuildings();
                return;
            }
            var customBuildingsExternalSavePath = Path.Combine(Constants.CurrentSavePath, "SolidFoundations", "buildings.json");

            // Get all known types for serialization
            Type[] knownTypes = Type.EmptyTypes;
            try
            {
                knownTypes = buildingManager.GetAllBuildingModels().Where(model => String.IsNullOrEmpty(model.IndoorMapType) is false && model.IndoorMapTypeAssembly.Equals("Stardew Valley", StringComparison.OrdinalIgnoreCase) is false).Select(model => Type.GetType($"{model.IndoorMapType},{model.IndoorMapTypeAssembly}")).ToArray();
                knownTypes = knownTypes.Concat(DGAIntegration.SpacecoreTypes).ToArray();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to get known types, some buildings may fail to load: {ex}", LogLevel.Trace);
            }

            var externallySavedCustomBuildings = new List<GenericBuilding>();
            try
            {
                // Get the externally saved custom building objects
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<GenericBuilding>), knownTypes);

                using (StreamReader textReader = new StreamReader(customBuildingsExternalSavePath))
                {
                    externallySavedCustomBuildings = (List<GenericBuilding>)xmlSerializer.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Monitor.Log($"Failed initial loading of custom buildings: {ex}", LogLevel.Trace);
                    Monitor.Log($"Attempting to remove SpaceCore-related items from the cached buildings...", LogLevel.Trace);

                    // Load the XML and remove any nodes with the xsi:type of "Mods_..."
                    XDocument doc = XDocument.Load(customBuildingsExternalSavePath);
                    var invalidNodes = doc.Descendants().Where(n => n.Attributes().FirstOrDefault(y => y.Name.LocalName == "type") is var nodeType && nodeType is not null && nodeType.Value.Contains("Mods_", StringComparison.OrdinalIgnoreCase)).Select(n => n).ToList();
                    invalidNodes.ForEach(x => x.Remove());

                    // Attempt the secondary deserialization
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<GenericBuilding>), knownTypes);
                    externallySavedCustomBuildings = (List<GenericBuilding>)xmlSerializer.Deserialize(doc.CreateReader());

                    Monitor.Log($"Cleanup of the cached buildings was successful!", LogLevel.Trace);
                }
                catch (Exception secondaryEx)
                {
                    Monitor.Log("Failed to load the cached custom buildings: No custom buildings will be loaded!", LogLevel.Warn);
                    Monitor.Log($"Failed to load the cached custom buildings: {secondaryEx}", LogLevel.Trace);

                    return;
                }
            }

            // Process each buildable location and restore any installed custom buildings
            foreach (GameLocation buildableLocation in Game1.locations.Where(l => l is GameLocation && l.modData.ContainsKey(ModDataKeys.LOCATION_CUSTOM_BUILDINGS)).ToList())
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

                        if (MigrateCachedCustomBuildingForLocation(buildableLocation, customBuilding) is false)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Failed to load cached custom building {archivedData.Id} at [{archivedData.TileX}, {archivedData.TileY}] within the map {buildableLocation.NameOrUniqueName}, see log for details.", LogLevel.Warn);
                        Monitor.Log($"Failure to load the custom building: {ex}", LogLevel.Trace);
                    }
                }
            }
        }

        private bool MigrateCachedCustomBuildingForLocation(GameLocation buildableLocation, GenericBuilding customBuilding)
        {
            try
            {
                GameLocation interior = null;
                if (customBuilding.indoors.Value is not null)
                {
                    interior = customBuilding.indoors.Value;

                    if (Game1.locations.Contains(interior) is false && buildableLocation is not Farm)
                    {
                        Game1.locations.Add(interior);
                    }
                }

                customBuilding.buildingType.Value = customBuilding.Id;

                // Set the location
                customBuilding.parentLocationName.Value = buildableLocation.NameOrUniqueName;

                // Load the building
                customBuilding.load();

                // Establish any buildings within this building
                if (interior is GameLocation subBuildableLocation && subBuildableLocation is not null && subBuildableLocation.buildings is not null)
                {
                    foreach (var subBuilding in subBuildableLocation.buildings.ToList())
                    {
                        if (subBuilding is GenericBuilding subCustomBuilding)
                        {
                            subBuildableLocation.buildings.Remove(subCustomBuilding);
                            MigrateCachedCustomBuildingForLocation(subBuildableLocation, subCustomBuilding);
                            continue;
                        }

                        // Handle vanilla buildings
                        subBuilding.load();
                        if (subBuilding.indoors.Value is not null && subBuilding.indoors.Value.warps is not null)
                        {
                            foreach (Warp warp in subBuilding.indoors.Value.warps)
                            {
                                warp.TargetName = subBuildableLocation.NameOrUniqueName;
                            }
                        }
                    }
                }

                // Restore the archived custom building
                buildableLocation.buildings.Add((Building)customBuilding);

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
                Monitor.Log($"Failed to setup cached custom building {customBuilding.Id} at [{customBuilding.tileX}, {customBuilding.tileY}] within the map {buildableLocation.NameOrUniqueName}, see log for details.", LogLevel.Warn);
                Monitor.Log($"Failed to setup cached custom building {customBuilding.Id} at [{customBuilding.tileX}, {customBuilding.tileY}] within the map {buildableLocation.NameOrUniqueName}: {ex}", LogLevel.Trace);
                return false;
            }

            return true;
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
                    LoadInteriors(contentPack, silent);

                    // Load the buildings
                    Monitor.Log($"Loading buildings from pack: {contentPack.Manifest.Name}", LogLevel.Trace);
                    LoadBuildings(contentPack, silent);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to load the content pack {contentPack.Manifest.UniqueID}: {ex}", LogLevel.Warn);
                }
            }
        }

        private void LoadInteriors(IContentPack contentPack, bool silent)
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

                var parentFolderName = "Interiors";
                foreach (var interiorFile in interiorFiles)
                {
                    // Cache the interior map
                    if (interiorFile.Exists)
                    {
                        var mapPath = contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, interiorFile.Name)).Name;
                        buildingManager.AddMapAsset(Path.GetFileNameWithoutExtension(interiorFile.Name), mapPath);

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

        private void LoadBuildings(IContentPack contentPack, bool silent)
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
                    ExtendedBuildingModel buildingModel = null;
                    try
                    {
                        buildingModel = contentPack.ReadJsonFile<ExtendedBuildingModel>(modelPath);
                    }
                    catch (Newtonsoft.Json.JsonReaderException)
                    {
                        // TODO: Add to compatibility alerts that the pack is using string-based Rectangles
                        Monitor.Log($"Attempting to resolve ExtendedBuildingModel read issue by handling string-based Rectangles for {modelPath}", LogLevel.Trace);

                        // Attempt to handle string-based Rectangle classes (SourceRect / AnimalDoor)
                        buildingModel = contentPack.ReadJsonFile<OldExtendedBuildingModel>(modelPath);
                    }

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(buildingModel.Name))
                    {
                        Monitor.Log($"Unable to add building from content pack {contentPack.Manifest.Name}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    if (buildingModel is null)
                    {
                        Monitor.Log($"Unable to add building from {modelPath}: The model is invalid", LogLevel.Warn);
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

                    // Load in the paint mask, if given
                    if (File.Exists(Path.Combine(folder.FullName, "paint_mask.png")))
                    {
                        var paintMaskPath = Path.Combine(parentFolderName, folder.Name, "paint_mask.png");
                        buildingModel.PaintMaskTexture = $"{buildingModel.ID}_BaseTexture_PaintMask";
                        buildingManager.AddTextureAsset(buildingModel.PaintMaskTexture, contentPack.ModContent.GetInternalAssetName(paintMaskPath).Name);

                        Monitor.Log($"Loaded the building {buildingModel.ID} PaintMask texture: {buildingModel.PaintMaskTexture} | {paintMaskPath}", LogLevel.Trace);
                    }

                    // Load in any skins, if given
                    if (Directory.Exists(Path.Combine(folder.FullName, "Skins")))
                    {
                        foreach (var skinPath in Directory.GetFiles(Path.Combine(folder.FullName, "Skins"), "*.png").OrderBy(s => s))
                        {
                            var localSkinPath = Path.Combine(parentFolderName, folder.Name, "Skins", Path.GetFileName(skinPath));
                            buildingManager.AddTextureAsset($"{buildingModel.ID}_Skins_{Path.GetFileNameWithoutExtension(skinPath)}", contentPack.ModContent.GetInternalAssetName(localSkinPath).Name);
                        }

                        if (buildingModel.Skins is not null)
                        {
                            foreach (var skin in buildingModel.Skins.Where(t => String.IsNullOrEmpty(t.Texture) is false))
                            {
                                var skinAsset = $"{buildingModel.ID}_Skins_{Path.GetFileNameWithoutExtension(skin.Texture)}";
                                if (buildingManager.GetTextureAsset(skinAsset) is null)
                                {
                                    Monitor.Log($"Unable to find the skin {skin.Texture} under Skins for {buildingModel.Name} from {contentPack.Manifest.Name}, skipping.", LogLevel.Trace);
                                    continue;
                                }

                                if (String.IsNullOrEmpty(skin.PaintMaskTexture) is false)
                                {
                                    var maskAsset = $"{buildingModel.ID}_Skins_{Path.GetFileNameWithoutExtension(skin.PaintMaskTexture)}";
                                    if (buildingManager.GetTextureAsset(maskAsset) is null)
                                    {
                                        Monitor.Log($"Unable to find the skin mask {skin.PaintMaskTexture} under Skins for {buildingModel.Name} from {contentPack.Manifest.Name}, skipping.", LogLevel.Trace);
                                    }
                                    else
                                    {
                                        skin.PaintMaskTexture = $"{skinAsset}_PaintMask";
                                        buildingManager.AddTextureAsset(skin.PaintMaskTexture, buildingManager.GetTextureAsset(maskAsset));

                                        Monitor.Log($"Loaded the building {buildingModel.ID} skin {skin.Id} mask texture: {skin.PaintMaskTexture}", LogLevel.Trace);
                                    }
                                }
                                else if (File.Exists(Path.Combine(folder.FullName, "paint_mask.png")))
                                {
                                    skin.PaintMaskTexture = $"{skinAsset}_PaintMask";
                                    buildingManager.AddTextureAsset(skin.PaintMaskTexture, contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, folder.Name, "paint_mask.png")).Name);

                                    Monitor.Log($"Loaded the building {buildingModel.ID} skin {skin.Id} mask texture using the default paint mask", LogLevel.Trace);
                                }

                                skin.Texture = skinAsset;
                                Monitor.Log($"Loaded the building {buildingModel.ID} Skins texture: {skin.Texture} | {skinAsset}", LogLevel.Trace);
                            }
                        }
                    }

                    // Load in the sprites for DrawLayers, if given
                    if (Directory.Exists(Path.Combine(folder.FullName, "Sprites")))
                    {
                        foreach (var spritePath in Directory.GetFiles(Path.Combine(folder.FullName, "Sprites"), "*.png"))
                        {
                            var localSpritePath = Path.Combine(parentFolderName, folder.Name, "Sprites", Path.GetFileName(spritePath));
                            buildingManager.AddTextureAsset($"{buildingModel.ID}_Sprites_{Path.GetFileNameWithoutExtension(spritePath)}", contentPack.ModContent.GetInternalAssetName(localSpritePath).Name);
                        }

                        if (buildingModel.DrawLayers is not null)
                        {
                            foreach (var layer in buildingModel.DrawLayers.Where(t => String.IsNullOrEmpty(t.Texture) is false))
                            {
                                var spriteAsset = $"{buildingModel.ID}_Sprites_{Path.GetFileNameWithoutExtension(layer.Texture)}";
                                if (buildingManager.GetTextureAsset(spriteAsset) is null)
                                {
                                    Monitor.Log($"Unable to find the texture {layer.Texture} under Sprites for {buildingModel.Name} from {contentPack.Manifest.Name}, assuming to be vanilla or a texture loaded via Content Patcher.", LogLevel.Trace);
                                    continue;
                                }

                                layer.Texture = spriteAsset;
                                Monitor.Log($"Loaded the building {buildingModel.ID} Sprites texture: {layer.Texture} | {spriteAsset}", LogLevel.Trace);
                            }
                        }
                    }

                    // Load in the texture
                    var texturePath = Path.Combine(parentFolderName, folder.Name, "building.png");
                    buildingModel.Texture = $"{buildingModel.ID}_BaseTexture";
                    buildingManager.AddTextureAsset(buildingModel.Texture, contentPack.ModContent.GetInternalAssetName(texturePath).Name);

                    // Add building's ID to texture tracker so we can quickly reference it for Content Patcher
                    buildingManager.AddTextureAsset(buildingModel.ID.ToLower(), buildingModel.Texture);

                    Monitor.Log($"Loaded the building texture {buildingModel.Texture} | {texturePath}", LogLevel.Trace);

                    // Handle setting HumanDoor, if AuxiliaryHumanDoors is populated but the former isn't
                    if (buildingModel.HumanDoor.X == -1 && buildingModel.HumanDoor.Y == -1 && buildingModel.AuxiliaryHumanDoors.Count > 0)
                    {
                        buildingModel.HumanDoor = buildingModel.AuxiliaryHumanDoors.First();
                    }

                    // Load in any provided translations
                    if (contentPack.Translation is not null)
                    {
                        buildingModel.Translations = contentPack.Translation;

                        buildingModel.Name = buildingModel.GetTranslation(buildingModel.Name);
                        buildingModel.Description = buildingModel.GetTranslation(buildingModel.Description);
                    }

                    // Check for any compatibility issues
                    HandleCompatibilityIssues(buildingModel, silent);

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

        private void HandleCompatibilityIssues(ExtendedBuildingModel model, bool silent)
        {
            if (model is null)
            {
                return;
            }

            if (string.Equals(model.Builder, "Carpenter", StringComparison.OrdinalIgnoreCase))
            {
                Monitor.Log($"{model.ID} is using the outdated value \"Carpenter\" for the \"Builder\" property. Solid Foundations will handle this, though the value should be changed to \"Robin\" for forward compatibility.", silent ? LogLevel.Trace : LogLevel.Warn);

                model.Builder = "Robin";
            }
            else if (model.MagicalConstruction is null && string.Equals(model.Builder, "Wizard", StringComparison.OrdinalIgnoreCase))
            {
                Monitor.Log($"{model.ID} is using the value \"Wizard\" for the \"Builder\" property, but has not declared the \"MagicalConstruction\" property. Solid Foundations will infer \"MagicalConstruction\" as true, though for forward compatibility this should be set manually.", silent ? LogLevel.Trace : LogLevel.Warn);

                model.MagicalConstruction = true;
            }
        }

        private void RefreshAllCustomBuildings(bool resetTexture = true)
        {
            foreach (GameLocation buildableLocation in Game1.locations.Where(l => l .buildings is not null))
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

        private void PlaceBuildingAtTile(string command, string[] args)
        {
            if (args.Length < 3)
            {
                Monitor.Log($"Missing required arguments: X Y", LogLevel.Warn);
                return;
            }

            var targetTile = Game1.player.Tile;
            if (args.Length > 2 && Int32.TryParse(args[1], out int xTile) && Int32.TryParse(args[2], out int yTile))
            {
                targetTile = new Vector2(xTile, yTile);
            }

            monitor.Log(api.ConstructBuildingImmediately(args[0], Game1.currentLocation, targetTile).Value.ToString(), LogLevel.Debug);
        }
    }
}
