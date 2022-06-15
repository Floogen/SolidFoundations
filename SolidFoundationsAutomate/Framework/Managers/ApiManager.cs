using SolidFoundationsAutomate.Framework.Interfaces;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundationsAutomate.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IAutomateApi _automateApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoAutomate(IModHelper helper)
        {
            _automateApi = helper.ModRegistry.GetApi<IAutomateApi>("Pathoschild.Automate");

            if (_automateApi is null)
            {
                _monitor.Log("Failed to hook into Pathoschild.Automate.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into Pathoschild.Automate.", LogLevel.Debug);
            return true;
        }

        public IAutomateApi GetAutomateApi()
        {
            return _automateApi;
        }
    }
}
