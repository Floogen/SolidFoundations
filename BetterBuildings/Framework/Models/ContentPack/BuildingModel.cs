using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public class BuildingModel
    {
        private string _displayName { get; set; }
        public string DisplayName { get { return String.IsNullOrEmpty(_displayName) ? Name : _displayName; } set { _displayName = value; } }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dimensions Dimensions { get; set; }
        public bool ShowShadow { get; set; } = true;
        public List<TileLocation> WalkableTiles { get; set; }
        public BlueprintModel Blueprint { get; set; }

        internal string Owner { get; set; }
        internal string PackName { get; set; }
        internal string Id { get; set; }
        internal Texture2D Texture { get; set; }
        internal Texture2D PaintMask { get; set; }

        internal void EstablishBlueprint()
        {
            if (Blueprint is not null)
            {
                Blueprint.AssociatedBuildingModel = this;
            }
        }

        internal GenericBuilding CreateBuilding()
        {
            var building = new GenericBuilding(Blueprint.CreateBlueprint())
            {
                Id = this.Id,
                WalkableTiles = this.WalkableTiles,
                ShowShadow = this.ShowShadow
            };

            building.buildingType.Value = ModDataKeys.GENERIC_BUILDING;
            building.texture = new Lazy<Texture2D>(delegate
            {
                return Texture;
            });

            return building;
        }

        internal GenericBuilding CreateBuilding(GameLocation gamelocation, Vector2 tileLocation)
        {
            var building = CreateBuilding();

            // Set the tile location
            building.tileX.Value = (int)tileLocation.X;
            building.tileY.Value = (int)tileLocation.Y;

            // Cache the GameLocation's name, as we want to allow for buildings outside the Farm
            building.LocationName = gamelocation.NameOrUniqueName;

            return building;
        }

        public override string ToString()
        {
            return $"Name: {Name} | Id: {Id} | Pack Name: {PackName}";
        }
    }
}
