using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Utilities
{
    internal static class Toolkit
    {
        internal static int GetLightSourceIdentifierForBuilding(Point tile, int count)
        {
            var baseId = (tile.X * 5000) + (tile.Y * 6000);
            return baseId + count + 1;
        }
    }
}
