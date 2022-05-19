using SolidFoundations.Framework.Models.ContentPack;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Managers
{
    internal class BuildingManager
    {
        private IMonitor _monitor;
        private IModHelper _helper;

        private List<ExtendedBuildingModel> _buildings;
        private Dictionary<string, string> _assetPathToMap;
        private Dictionary<string, string> _assetPathToTexture;

        public BuildingManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;

            _buildings = new List<ExtendedBuildingModel>();
            _assetPathToMap = new Dictionary<string, string>();
            _assetPathToTexture = new Dictionary<string, string>();
        }

        public void Reset()
        {
            _buildings.Clear();
            _assetPathToMap.Clear();
            _assetPathToTexture.Clear();
        }

        public void AddBuilding(ExtendedBuildingModel model)
        {
            if (_buildings.FirstOrDefault(t => t.ID.Equals(model.ID, StringComparison.OrdinalIgnoreCase)) is ExtendedBuildingModel buildingExtended && buildingExtended is not null)
            {
                var replacementIndex = _buildings.IndexOf(buildingExtended);
                _buildings[replacementIndex] = model;
            }
            else
            {
                _buildings.Add(model);
            }
        }

        public void AddMapAsset(string assetPath, string pathToMap)
        {
            if (String.IsNullOrEmpty(assetPath) || String.IsNullOrEmpty(pathToMap))
            {
                return;
            }

            _assetPathToMap[assetPath] = pathToMap;
        }

        public void AddTextureAsset(string assetPath, string pathToTexture)
        {
            if (String.IsNullOrEmpty(assetPath) || String.IsNullOrEmpty(pathToTexture))
            {
                return;
            }

            _assetPathToTexture[assetPath] = pathToTexture;
        }

        public string GetMapAsset(string assetPath)
        {
            if (_assetPathToMap.ContainsKey(assetPath))
            {
                return _assetPathToMap[assetPath];
            }

            return null;
        }

        public string GetTextureAsset(string assetPath)
        {
            if (_assetPathToTexture.ContainsKey(assetPath))
            {
                return _assetPathToTexture[assetPath];
            }

            return null;
        }

        public List<ExtendedBuildingModel> GetAllBuildingModels()
        {
            return _buildings;
        }

        public T GetSpecificBuildingModel<T>(string buildingId) where T : ExtendedBuildingModel
        {
            return (T)_buildings.FirstOrDefault(t => String.Equals(t.ID, buildingId, StringComparison.OrdinalIgnoreCase) && t is T);
        }

        public bool DoesBuildingModelExist(string buildingId)
        {
            return _buildings.Any(t => String.Equals(t.ID, buildingId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
