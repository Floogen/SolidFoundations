using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Utilities
{
    internal static class FlexibleLocationFinder
    {
        public static BuildableGameLocation GetBuildableLocationByName(string name, bool noMatchReturnsFarm = true)
        {
            var location = Game1.getLocationFromName(name);
            if (location is BuildableGameLocation buildableGameLocation && buildableGameLocation is not null)
            {
                return buildableGameLocation;
            }

            return noMatchReturnsFarm ? Game1.getFarm() : null;
        }
    }
}
