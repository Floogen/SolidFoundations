using BetterBuildings.Framework.Models.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public class GenericBuilding : Building
    {
        public string Id { get; set; }
        public string LocationName { get; set; }
        public Dimensions Dimensions { get { return new Dimensions() { Height = base.tilesHigh.Value, Width = base.tilesWide.Value }; } }
        public TileLocation TileLocation { get { return new TileLocation() { X = base.tileX.Value, Y = base.tileY.Value }; } }

        public GenericBuilding() : base()
        {

        }

        public GenericBuilding(GenericBlueprint genericBlueprint) : base(genericBlueprint, Vector2.Zero)
        {

        }
    }
}
