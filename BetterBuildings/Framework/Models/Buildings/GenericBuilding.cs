using BetterBuildings.Framework.Models.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
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
        public BuildingModel Model { get; set; }
        public string Id { get { return Model is null ? String.Empty : Model.Id; } }
        public string LocationName { get; set; }
        public TileLocation TileLocation { get { return new TileLocation() { X = base.tileX.Value, Y = base.tileY.Value }; } }

        public GenericBuilding() : base()
        {

        }

        public GenericBuilding(BuildingModel model, GenericBlueprint genericBlueprint) : base(genericBlueprint, Vector2.Zero)
        {
            Model = model;
        }

        public void RefreshModel(BuildingModel model)
        {
            Model = model;
        }

        private bool IsTileToTheRightWalkable(TileLocation tileLocation)
        {
            return Model.WalkableTiles.Any(t => t.X == tileLocation.X + 1 && t.Y == tileLocation.Y);
        }

        public override bool intersects(Rectangle boundingBox)
        {
            if (base.daysOfConstructionLeft.Value > 0)
            {
                return base.intersects(boundingBox);
            }
            else if (base.intersects(boundingBox))
            {
                foreach (var tileLocation in Model.WalkableTiles)
                {
                    if (boundingBox.X >= (tileLocation.X + base.tileX.Value) * 64 && boundingBox.Right <= (tileLocation.X + base.tileX.Value + 1) * 64 + (IsTileToTheRightWalkable(tileLocation) ? 64 : 0))
                    {
                        return boundingBox.Y <= (tileLocation.Y + base.tileY.Value) * 64 && boundingBox.Bottom <= (tileLocation.Y + base.tileY.Value - 1) * 64;
                    }
                }

                return true;
            }

            return base.intersects(boundingBox);
        }

        public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
        {
            if (Model.ShowShadow)
            {
                base.drawShadow(b, localX, localY);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!base.isMoving)
            {
                if (base.daysOfConstructionLeft.Value > 0)
                {
                    this.drawInConstruction(b);
                    return;
                }

                this.drawShadow(b);
                b.Draw(base.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileX.Value * 64, base.tileY.Value * 64 + base.tilesHigh.Value * 64)), base.texture.Value.Bounds, base.color.Value * base.alpha.Value, 0f, new Vector2(0f, base.texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)((base.tileY.Value + base.tilesHigh.Value - 1) * 64) / 10000f);
            }
        }
    }
}
