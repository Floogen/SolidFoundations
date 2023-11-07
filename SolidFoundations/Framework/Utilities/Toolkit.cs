using Microsoft.Xna.Framework;
using System;

namespace SolidFoundations.Framework.Utilities
{
    internal static class Toolkit
    {
        internal static int GetLightSourceIdentifierForBuilding(Point tile, int count)
        {
            var baseId = (tile.X * 5000) + (tile.Y * 6000);
            return baseId + count + 1;
        }

        internal static Rectangle GetRectangleFromString(string rawRectangle)
        {
            Rectangle rectangle;
            try
            {
                string[] array = rawRectangle.Split(' ');
                rectangle = new Rectangle(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), int.Parse(array[3]));
            }
            catch (Exception)
            {
                rectangle = Rectangle.Empty;
            }

            return rectangle;
        }
    }
}
