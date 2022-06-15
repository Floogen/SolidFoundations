using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundationsAutomate.Framework.Interfaces
{
    public interface IAutomateApi
    {
        /// <summary>Add an automation factory.</summary>
        /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
        void AddFactory(IAutomationFactory factory);

        /// <summary>Get the status of machines in a tile area. This is a specialized API for Data Layers and similar mods.</summary>
        /// <param name="location">The location for which to display data.</param>
        /// <param name="tileArea">The tile area for which to display data.</param>
        IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea);
    }
}
