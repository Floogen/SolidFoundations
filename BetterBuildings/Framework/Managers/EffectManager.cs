using BetterBuildings.Framework.Models.ContentPack;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Managers
{
    internal class EffectManager
    {
        private IMonitor _monitor;
        private List<EffectModel> _effects;

        public EffectManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _effects = new List<EffectModel>();
        }

        public void Reset()
        {
            _effects.Clear();
        }

        public void AddEffect(EffectModel model)
        {
            if (_effects.Any(t => t.Id == model.Id))
            {
                var replacementIndex = _effects.IndexOf(_effects.First(t => t.Id == model.Id));
                _effects[replacementIndex] = model;
            }
            else
            {
                _effects.Add(model);
            }
        }

        public List<EffectModel> GetAllEffectModels()
        {
            return _effects;
        }

        public T GetSpecificEffectModel<T>(string effectId) where T : EffectModel
        {
            return (T)_effects.FirstOrDefault(t => String.Equals(t.Id, effectId, StringComparison.OrdinalIgnoreCase) && t is T);
        }

        public bool DoesEffectModelExist(string effectId)
        {
            return _effects.Any(t => String.Equals(t.Id, effectId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
