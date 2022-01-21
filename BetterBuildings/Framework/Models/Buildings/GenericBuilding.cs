using BetterBuildings.Framework.Models.General.Tiles;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
        public string Id { get; set; }
        public string LocationName { get; set; }
        public TileLocation TileLocation { get { return new TileLocation() { X = base.tileX.Value, Y = base.tileY.Value }; } }

        internal bool IsUsingEventOverride { get; set; }
        internal bool DrawOverPlayer { get; set; }
        internal float? AlphaOverride { get; set; }

        private BoundaryCollective _walkableTileGroup;
        private BoundaryCollective _buildingTileGroup;

        private Texture2D _lavaTexture;
        private bool _lavaTileFlip;
        private float _lavaPosition;

        private int _idleAnimationTimer;
        private int _lastAnimationIndex;


        public GenericBuilding() : base()
        {

        }

        public GenericBuilding(BuildingModel model, GenericBlueprint genericBlueprint) : base(genericBlueprint, Vector2.Zero)
        {
            RefreshModel(model);

            base.indoors.Value = GetIndoors();
            this.updateInteriorWarps();
        }

        public void RefreshModel()
        {
            if (Model is not null)
            {
                RefreshModel(Model);
            }
        }

        public void RefreshModel(BuildingModel model)
        {
            Model = model;
            Id = model.Id;

            base.tilesHigh.Value = model.PhysicalDimensions.Height;
            base.tilesWide.Value = model.PhysicalDimensions.Width;
            base.fadeWhenPlayerIsBehind.Value = model.Fade.Enabled;

            _walkableTileGroup = new BoundaryCollective();
            foreach (var walkableTile in Model.WalkableTiles)
            {
                foreach (var tile in walkableTile.GetActualTiles())
                {
                    var adjustedTile = tile.GetAdjustedLocation(base.tileX.Value, base.tileY.Value);
                    _walkableTileGroup.Add(new Rectangle(adjustedTile.X * 64, adjustedTile.Y * 64, 64, 64));
                }
            }

            _buildingTileGroup = new BoundaryCollective();
            for (int x = 0; x < base.tilesWide.Value; x++)
            {
                for (int y = 0; y < base.tilesHigh.Value; y++)
                {
                    var boundaryTile = new Rectangle((base.tileX.Value + x) * 64, (base.tileY.Value + y) * 64, 64, 64);
                    if (!_walkableTileGroup.Contains(boundaryTile.X, boundaryTile.Y))
                    {
                        _buildingTileGroup.Add(boundaryTile);
                    }
                }
            }

            _lavaTexture = BetterBuildings.modHelper.Content.Load<Texture2D>("Maps/Mines/volcano_dungeon", ContentSource.GameContent);
        }

        public GameLocation GetIndoors()
        {
            if (Model is not null && !String.IsNullOrEmpty(Model.MapPath))
            {
                var indoorLocation = new GameLocation(Model.MapPath, Model.Id);
                indoorLocation.uniqueName.Value = Model.Id + Guid.NewGuid().ToString();

                if (Model.InteriorType is InteriorType.Greenhouse)
                {
                    indoorLocation.IsGreenhouse = true;
                }
                else if (Model.InteriorType is InteriorType.Coop or InteriorType.Barn)
                {
                    indoorLocation.IsFarm = true;
                }
                indoorLocation.isStructure.Value = true;

                return indoorLocation;
            }

            return null;
        }

        private bool AttemptTunnelDoorTeleport(TileLocation triggeredTile)
        {
            if (Model.Doorways is null || !Model.Doorways.Any(d => d.Type is DoorType.Tunnel))
            {
                return false;
            }

            var tunnelDoorway = Model.Doorways.First(d => d.Type == DoorType.Tunnel);
            foreach (var tile in tunnelDoorway.Tiles)
            {
                if (base.tileX.Value + tile.X == triggeredTile.X && base.tileY.Value + tile.Y == triggeredTile.Y)
                {
                    // Warp player inside
                    base.indoors.Value.isStructure.Value = true;
                    Game1.player.currentLocation.playSoundAt("doorClose", Game1.player.getTileLocation());

                    // Get warp destination tile
                    var destinationTile = new TileLocation() { X = this.indoors.Value.warps[0].X, Y = this.indoors.Value.warps[0].Y - 1 };
                    if (tunnelDoorway.EntranceTile is not null)
                    {
                        destinationTile = tunnelDoorway.EntranceTile;
                    }

                    Game1.warpFarmer(this.indoors.Value.uniqueName.Value, destinationTile.X, destinationTile.Y, Game1.player.FacingDirection, isStructure: true);
                    return true;
                }
            }

            return false;
        }

        private bool AttemptEventTileTrigger(TileLocation triggeredTile)
        {
            if (Model.EventTiles is null || Model.EventTiles.Count <= 0)
            {
                return false;
            }

            foreach (var eventTile in Model.EventTiles)
            {
                foreach (var tile in eventTile.GetActualTiles())
                {
                    if (base.tileX.Value + tile.X == triggeredTile.X && base.tileY.Value + tile.Y == triggeredTile.Y)
                    {
                        // Trigger the tile
                        eventTile.Trigger(this, Game1.player);

                        return true;
                    }
                }
            }

            return false;
        }

        private void ResetEventOverrides()
        {
            AlphaOverride = null;
            DrawOverPlayer = false;

            IsUsingEventOverride = false;
        }

        public override bool isActionableTile(int xTile, int yTile, Farmer who)
        {
            if (Model.InteractiveTiles is null || Model.InteractiveTiles.Count <= 0)
            {
                return false;
            }

            foreach (var interactiveTile in Model.InteractiveTiles)
            {
                if (base.tileX.Value + interactiveTile.Tile.X == xTile && base.tileY.Value + interactiveTile.Tile.Y == yTile)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool intersects(Rectangle boundingBox)
        {
            if (base.daysOfConstructionLeft.Value > 0)
            {
                return base.intersects(boundingBox);
            }

            if (base.intersects(boundingBox))
            {
                var buildingBounds = new Rectangle(base.tileX.Value * 64, base.tileY.Value * 64, base.tilesWide.Value * 64, base.tilesHigh.Value * 64);
                if (_walkableTileGroup.ContainsAtLeastOnePoint(boundingBox))
                {
                    // These only applies to player inside walkable polygon
                    if (!_buildingTileGroup.Intersects(boundingBox) && _walkableTileGroup.Intersects(boundingBox))
                    {
                        return false;
                    }
                    else if (!buildingBounds.Contains(boundingBox) && _walkableTileGroup.ContainsAtLeastHalf(boundingBox))
                    {
                        return false;
                    }
                }

                return true;
            }

            return base.intersects(boundingBox);
        }

        public override void updateInteriorWarps(GameLocation interior = null)
        {
            if (interior is null)
            {
                if (this.indoors.Value is null)
                {
                    return;
                }

                interior = this.indoors.Value;
            }

            if (Model.Doorways is not null)
            {
                TileLocation exitTile = new TileLocation() { X = 0, Y = base.tilesHigh.Value };
                if (Model.Doorways.FirstOrDefault(d => d.Type is DoorType.Standard or DoorType.Tunnel) is DoorTiles humanDoorway && humanDoorway is not null && humanDoorway.ExitTile is not null)
                {
                    exitTile = humanDoorway.ExitTile;
                }

                foreach (Warp warp in interior.warps)
                {
                    warp.TargetX = base.tileX.Value + exitTile.X;
                    warp.TargetY = base.tileY.Value + exitTile.Y;
                }
            }
        }

        public override void Update(GameTime time)
        {
            // Lava update
            _lavaPosition += (float)((Math.Sin((float)time.TotalGameTime.Milliseconds / 1000f) + 1.0) * 0.15000000596046448);
            if (_lavaPosition >= 64f)
            {
                _lavaPosition -= 64f;
                _lavaTileFlip = !_lavaTileFlip;
            }

            // Handle alpha for fading
            if (AlphaOverride is not null)
            {
                this.alpha.Value = Math.Max(AlphaOverride.Value, this.alpha.Value - 0.09f);
            }
            else
            {
                int adjustedTilesHigh = base.tilesHigh.Value;
                if (Model.Fade.MinTileHeightBeforeFade >= 0)
                {
                    adjustedTilesHigh = Model.Fade.MinTileHeightBeforeFade;
                }

                var isPlayerNearTopOfBuilding = Game1.player.GetBoundingBox().Intersects(new Rectangle(64 * this.tileX.Value, 64 * (this.tileY.Value + this.tilesHigh.Value - adjustedTilesHigh), this.tilesWide.Value * 64, adjustedTilesHigh * 64));
                if (this.fadeWhenPlayerIsBehind.Value && isPlayerNearTopOfBuilding)
                {
                    this.alpha.Value = Math.Max(Model.Fade.AmountToFade == -1f ? 0.4f : Model.Fade.AmountToFade, this.alpha.Value - 0.09f);
                }
            }
            this.alpha.Value = Math.Min(1f, this.alpha.Value + 0.05f);

            // Handle any tile based events
            ResetEventOverrides();
            if (!Game1.isWarping && _walkableTileGroup.GetRectangleByPoint(Game1.player.GetBoundingBox()) is Rectangle walkableRectangle)
            {
                var walkableTile = new TileLocation() { X = walkableRectangle.X / 64, Y = walkableRectangle.Y / 64 };
                if (!AttemptTunnelDoorTeleport(walkableTile))
                {
                    AttemptEventTileTrigger(new TileLocation() { X = walkableRectangle.X / 64, Y = walkableRectangle.Y / 64 });
                }
            }

            // Handle updating any animations
            _idleAnimationTimer -= time.ElapsedGameTime.Milliseconds;
            if (Model.IdleAnimation.Count > 0 && _idleAnimationTimer <= 0)
            {
                _lastAnimationIndex = Model.IdleAnimation.Count <= _lastAnimationIndex + 1 ? 0 : _lastAnimationIndex + 1;
                _idleAnimationTimer = Model.IdleAnimation[_lastAnimationIndex].Duration;
            }
        }

        public override bool doAction(Vector2 tileLocation, Farmer who)
        {
            if (Model.InteractiveTiles is null || Model.InteractiveTiles.Count <= 0)
            {
                return false;
            }

            foreach (var interactiveTile in Model.InteractiveTiles)
            {
                if (base.tileX.Value + interactiveTile.Tile.X == tileLocation.X && base.tileY.Value + interactiveTile.Tile.Y == tileLocation.Y)
                {
                    // Trigger the tile
                    interactiveTile.Trigger(this, Game1.player);

                    return true;
                }
            }

            return base.doAction(tileLocation, who);
        }

        public override Rectangle getSourceRect()
        {
            int x = 0;
            int y = 0;
            int width = Model.TextureDimensions.Width * 16;
            int height = Model.TextureDimensions.Height * 16;

            if (Model.IdleAnimation.Count > 0)
            {
                x = Model.IdleAnimation[_lastAnimationIndex].Frame * width;
                y = Model.IdleAnimation[_lastAnimationIndex].RowOffset * height;
            }

            return new Rectangle(x, y, width, height);
        }

        public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
        {
            if (Model.ShowShadow)
            {
                base.drawShadow(b, localX, localY);
            }
        }

        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            if (base.tilesWide.Value <= 8)
            {
                this.drawShadow(b, x, y);
                b.Draw(base.texture.Value, new Vector2(x, y), this.getSourceRect(), base.color.Value, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
            }
            else if (base.tilesWide.Value >= 10)
            {
                int xOffset = this.getSourceRect().Width / 2;
                int yOffset = this.getSourceRect().Height / 2;
                b.Draw(base.texture.Value, new Vector2(x + xOffset, y + yOffset), this.getSourceRect(), base.color.Value, 0f, new Vector2(0f, 0f), 3f, SpriteEffects.None, 0.89f);
            }
            else
            {
                int xOffset = 108;
                int yOffset = 28;
                b.Draw(base.texture.Value, new Vector2(x + xOffset, y + yOffset), new Rectangle(this.getSourceRect().Width / 2 - 64, this.getSourceRect().Height - 136 - 2, 122, 138), base.color.Value, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
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

                foreach (var waterTile in Model.WaterTiles.Where(w => w.IsValid()))
                {
                    var tileGrid = waterTile.GetActualGrid();

                    // Iterate through each water tile
                    foreach (var gridTile in tileGrid.GetTiles())
                    {
                        var adjustedWaterTile = gridTile.GetAdjustedLocation(base.tileX.Value, base.tileY.Value);

                        int x = adjustedWaterTile.X;
                        int y = adjustedWaterTile.Y;
                        bool num = y == base.tileY.Value + tileGrid.StartingTile.Y + tileGrid.Height - 1;
                        bool topY = y == base.tileY.Value + tileGrid.StartingTile.Y;

                        if (waterTile.IsLava && _lavaTexture is not null)
                        {
                            int lava_tile_upper_x = 0;
                            int lava_tile_upper_y = 320;

                            b.Draw(_lavaTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!topY) ? _lavaPosition : 0f))), new Rectangle(lava_tile_upper_x + Game1.currentLocation.waterAnimationIndex * 16, lava_tile_upper_y + (((x + y) % 2 != 0) ? ((!_lavaTileFlip) ? 32 : 0) : (_lavaTileFlip ? 32 : 0)) + (topY ? ((int)_lavaPosition / 4) : 0), 16, 16 + (topY ? ((int)(0f - _lavaPosition) / 4) : 0)), waterTile.ActualColor.Equals(Color.White) ? Color.White : (waterTile.ActualColor * 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, ((base.tileY.Value - 0.5f) * 64f - 2f) / 10000f);
                            if (num)
                            {
                                b.Draw(_lavaTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)_lavaPosition)), new Rectangle(lava_tile_upper_x + Game1.currentLocation.waterAnimationIndex * 16, lava_tile_upper_y + (((x + (y + 1)) % 2 != 0) ? ((!_lavaTileFlip) ? 32 : 0) : (_lavaTileFlip ? 32 : 0)), 16, 16 - (int)(16f - _lavaPosition / 4f) - 1), waterTile.ActualColor.Equals(Color.White) ? Color.White : (waterTile.ActualColor * 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, ((base.tileY.Value - 0.5f) * 64f - 2f) / 10000f);
                            }
                        }
                        else
                        {
                            int water_tile_upper_x = 0;
                            int water_tile_upper_y = 2064;

                            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f))), new Rectangle(water_tile_upper_x + Game1.currentLocation.waterAnimationIndex * 64, water_tile_upper_y + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)) + (topY ? ((int)Game1.currentLocation.waterPosition / 4) : 0), 64, 64 + (topY ? ((int)(0f - Game1.currentLocation.waterPosition)) : 0)), waterTile.ActualColor.Equals(Color.White) ? (Game1.currentLocation.waterColor.Value) : (waterTile.ActualColor * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, ((base.tileY.Value - 0.5f) * 64f - 2f) / 10000f);
                            if (num)
                            {
                                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)Game1.currentLocation.waterPosition)), new Rectangle(water_tile_upper_x + Game1.currentLocation.waterAnimationIndex * 64, water_tile_upper_y + (((x + (y + 1)) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 64, 64 - (int)(64f - Game1.currentLocation.waterPosition) - 1), waterTile.ActualColor.Equals(Color.White) ? (Game1.currentLocation.waterColor.Value) : (waterTile.ActualColor * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, ((base.tileY.Value - 0.5f) * 64f - 2f) / 10000f);
                            }
                        }
                    }
                }

                this.drawShadow(b);
                b.Draw(base.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileX.Value * 64, base.tileY.Value * 64 + base.tilesHigh.Value * 64)), this.getSourceRect(), base.color.Value * base.alpha.Value, 0f, new Vector2(0f, Model.TextureDimensions.Height * 16), 4f, SpriteEffects.None, (float)((base.tileY.Value) * 64) / (DrawOverPlayer ? 8000f : 10000f));

                // Check if player's bounding box should be drawn
                if (BetterBuildings.showWalkableTiles || BetterBuildings.showBuildingTiles || BetterBuildings.showFadeBox)
                {
                    var playerPosition = Game1.GlobalToLocal(Game1.viewport, new Vector2((Game1.player.GetBoundingBox().X), (Game1.player.GetBoundingBox().Y)));
                    b.Draw(Game1.staminaRect, new Rectangle((int)playerPosition.X, (int)playerPosition.Y, Game1.player.GetBoundingBox().Width, Game1.player.GetBoundingBox().Height), new Rectangle(0, 0, 1, 1), Color.Blue, 0f, Vector2.Zero, SpriteEffects.None, 100f);
                }

                // Check if any of the debug boxes need to be drawn
                if (BetterBuildings.showWalkableTiles)
                {
                    foreach (var boundary in _walkableTileGroup.boundaries)
                    {
                        var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(boundary.X, boundary.Y));
                        b.Draw(Game1.staminaRect, position, new Rectangle(0, 0, 1, 1), Color.Red, 0f, Vector2.Zero, 64, SpriteEffects.None, 10f);
                    }
                }
                if (BetterBuildings.showBuildingTiles)
                {
                    foreach (var boundary in _buildingTileGroup.boundaries)
                    {
                        var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(boundary.X, boundary.Y));
                        b.Draw(Game1.staminaRect, position, new Rectangle(0, 0, 1, 1), Color.Yellow, 0f, Vector2.Zero, 64, SpriteEffects.None, 10f);
                    }
                }
                if (BetterBuildings.showFadeBox)
                {
                    int adjustedTilesHigh = base.tilesHigh.Value;
                    if (Model.Fade.MinTileHeightBeforeFade >= 0)
                    {
                        adjustedTilesHigh = Model.Fade.MinTileHeightBeforeFade;
                    }

                    var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(64 * this.tileX.Value, 64 * (this.tileY.Value + this.tilesHigh.Value - adjustedTilesHigh)));
                    b.Draw(Game1.staminaRect, new Rectangle((int)position.X, (int)position.Y, this.tilesWide.Value * 64, adjustedTilesHigh * 64), new Rectangle(0, 0, 1, 1), Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, 9f);
                }
            }
        }
    }
}
