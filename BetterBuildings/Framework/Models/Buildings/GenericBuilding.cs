using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Buildings
{
    internal class GenericBuilding
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public GenericBlueprint Blueprint { get; set; }

        internal string Owner { get; set; }
        internal string PackName { get; set; }
        internal string Id { get; set; }
        internal Texture2D Texture { get; set; }
        internal Texture2D PaintMask { get; set; }

        internal void EstablishBlueprint()
        {
            if (Blueprint is not null)
            {
                Blueprint.Setup(this);
            }
        }

        public override string ToString()
        {
            return $"Name: {Name} | Id: {Id} | Pack Name: {PackName}";
        }
    }
}
