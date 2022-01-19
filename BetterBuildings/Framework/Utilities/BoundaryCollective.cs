using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Utilities
{
    public class BoundaryCollective
    {
        internal List<Rectangle> boundaries;

        public BoundaryCollective()
        {
            boundaries = new List<Rectangle>();
        }

        public void Add(Rectangle rectangle)
        {
            boundaries.Add(rectangle);
        }

        public bool Intersects(Rectangle rectangle)
        {
            foreach (Rectangle boundary in this.boundaries)
            {
                if (boundary.Intersects(rectangle))
                {
                    return true;
                }
            }
            return false;
        }

        public Rectangle? GetRectangleByPoint(int x, int y)
        {
            foreach (Rectangle boundary in this.boundaries)
            {
                if (boundary.Contains(x, y))
                {
                    return boundary;
                }
            }

            return null;
        }

        public Rectangle? GetRectangleByPoint(Rectangle rectangle)
        {
            var boundary = this.GetRectangleByPoint(rectangle.Left, rectangle.Top);
            if (boundary is not null)
            {
                return boundary;
            }

            boundary = this.GetRectangleByPoint(rectangle.Right, rectangle.Top);
            if (boundary is not null)
            {
                return boundary;
            }

            boundary = this.GetRectangleByPoint(rectangle.Left, rectangle.Bottom);
            if (boundary is not null)
            {
                return boundary;
            }

            boundary = this.GetRectangleByPoint(rectangle.Right, rectangle.Bottom);
            if (boundary is not null)
            {
                return boundary;
            }

            return null;
        }

        public bool Contains(int x, int y)
        {
            foreach (Rectangle boundary in this.boundaries)
            {
                if (boundary.Contains(x, y))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(Rectangle rectangle)
        {
            if (!this.Contains(rectangle.Left, rectangle.Top))
            {
                return false;
            }
            if (!this.Contains(rectangle.Right, rectangle.Top))
            {
                return false;
            }
            if (!this.Contains(rectangle.Left, rectangle.Bottom))
            {
                return false;
            }
            if (!this.Contains(rectangle.Right, rectangle.Bottom))
            {
                return false;
            }

            return true;
        }

        public bool ContainsAtLeastHalf(Rectangle rectangle, int direction = -1)
        {
            int contactedEdges = 0;
            if (this.Contains(rectangle.Left, rectangle.Top))
            {
                if (direction == -1 || direction == 0 || direction == 3)
                {
                    contactedEdges += 1;
                }
            }
            if (this.Contains(rectangle.Right, rectangle.Top))
            {
                if (direction == -1 || direction == 0 || direction == 1)
                {
                    contactedEdges += 1;
                }
            }
            if (this.Contains(rectangle.Left, rectangle.Bottom))
            {
                if (direction == -1 || direction == 2 || direction == 3)
                {
                    contactedEdges += 1;
                }
            }
            if (this.Contains(rectangle.Right, rectangle.Bottom))
            {
                if (direction == -1 || direction == 2 || direction == 1)
                {
                    contactedEdges += 1;
                }
            }

            return contactedEdges >= 2;
        }

        public bool ContainsAtLeastOnePoint(Rectangle rectangle, int direction = -1)
        {
            int contactedEdges = 0;
            if (this.Contains(rectangle.Left, rectangle.Top))
            {
                if (direction == -1 || direction == 0 || direction == 3)
                {
                    contactedEdges += 1;
                }
            }
            if (this.Contains(rectangle.Right, rectangle.Top))
            {
                if (direction == -1 || direction == 0 || direction == 1)
                {
                    contactedEdges += 1;
                }
            }
            if (this.Contains(rectangle.Left, rectangle.Bottom))
            {
                if (direction == -1 || direction == 2 || direction == 3)
                {
                    contactedEdges += 1;
                }
            }
            if (this.Contains(rectangle.Right, rectangle.Bottom))
            {
                if (direction == -1 || direction == 2 || direction == 1)
                {
                    contactedEdges += 1;
                }
            }

            return contactedEdges >= 1;
        }
    }
}
