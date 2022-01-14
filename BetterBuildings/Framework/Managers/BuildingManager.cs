using BetterBuildings.Framework.Models.ContentPack;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Managers
{
    internal class BuildingManager
    {
        private IMonitor _monitor;
        private List<BuildingModel> _buildings;

        public BuildingManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _buildings = new List<BuildingModel>();
        }

        public void Reset()
        {
            _buildings.Clear();
        }

        public void AddBuilding(BuildingModel model)
        {
            if (_buildings.Any(t => t.Id == model.Id))
            {
                var replacementIndex = _buildings.IndexOf(_buildings.First(t => t.Id == model.Id));
                _buildings[replacementIndex] = model;
            }
            else
            {
                _buildings.Add(model);
            }
        }

        public List<BuildingModel> GetAllBuildingModels()
        {
            return _buildings;
        }

        public T GetSpecificBuildingModel<T>(string buildingId) where T : BuildingModel
        {
            return (T)_buildings.FirstOrDefault(t => String.Equals(t.Id, buildingId, StringComparison.OrdinalIgnoreCase) && t is T);
        }

        public bool DoesBuildingModelExist(string buildingId)
        {
            return _buildings.Any(t => String.Equals(t.Id, buildingId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
