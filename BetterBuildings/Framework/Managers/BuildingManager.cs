using BetterBuildings.Framework.Models.Buildings;
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
        private List<GenericBuilding> _buildings;

        public BuildingManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _buildings = new List<GenericBuilding>();
        }

        public void Reset()
        {
            _buildings.Clear();
        }

        public void AddBuilding(GenericBuilding model)
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

        public List<GenericBuilding> GetAllBuildingModels()
        {
            return _buildings;
        }

        public T GetSpecificBuildingModel<T>(string buildingId) where T : GenericBuilding
        {
            return (T)_buildings.FirstOrDefault(t => String.Equals(t.Id, buildingId, StringComparison.OrdinalIgnoreCase) && t is T);
        }
    }
}
