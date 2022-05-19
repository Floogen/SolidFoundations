using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SolidFoundations.Framework.Models.Backport;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Object = StardewValley.Object;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class GenericBuilding : Building
    {
        public ExtendedBuildingModel Model { get; set; }
        public string Id { get; set; }
        public GameLocation Location { get; set; }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.buildingChests
        public NetList<Chest, NetRef<Chest>> buildingChests = new NetList<Chest, NetRef<Chest>>();
        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.animalDoorOpenAmount
        public readonly NetFloat animalDoorOpenAmount = new NetFloat();

        private Texture2D _lavaTexture;

        public GenericBuilding() : base()
        {

        }

        public GenericBuilding(ExtendedBuildingModel model) : base(new BluePrint(model.ID), Vector2.Zero)
        {
            RefreshModel(model);
        }

        public GenericBuilding(ExtendedBuildingModel model, BluePrint bluePrint) : base(bluePrint, Vector2.Zero)
        {
            RefreshModel(model);

            base.texture = new Lazy<Texture2D>(bluePrint.texture);
        }

        public GenericBuilding(ExtendedBuildingModel model, BluePrint bluePrint, Vector2 tileLocation) : base(bluePrint, tileLocation)
        {
            RefreshModel(model);

            base.texture = new Lazy<Texture2D>(bluePrint.texture);
        }

        public void RefreshModel()
        {
            if (Model is not null)
            {
                RefreshModel(Model);
            }
        }

        public void RefreshModel(ExtendedBuildingModel model)
        {
            Model = model;
            Id = model.ID;

            base.tilesWide.Value = model.Size.X;
            base.tilesHigh.Value = model.Size.Y;
            base.fadeWhenPlayerIsBehind.Value = model.FadeWhenBehind;

            _lavaTexture = SolidFoundations.modHelper.GameContent.Load<Texture2D>("Maps/Mines/volcano_dungeon");
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.ApplySourceRectOffsets
        public virtual Rectangle ApplySourceRectOffsets(Rectangle source)
        {
            if (this.Model.SeasonOffset != Point.Zero)
            {
                int num = 0;
                if (Game1.IsSpring)
                {
                    num = 0;
                }
                else if (Game1.IsSummer)
                {
                    num = 1;
                }
                else if (Game1.IsFall)
                {
                    num = 2;
                }
                else if (Game1.IsWinter)
                {
                    num = 3;
                }
                source.X += this.Model.SeasonOffset.X * num;
                source.Y += this.Model.SeasonOffset.Y * num;
            }

            return source;
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.getSourceRect
        public override Rectangle getSourceRect()
        {
            Rectangle rectangle = this.Model.GetSourceRect();
            if (rectangle == Rectangle.Empty)
            {
                return base.texture.Value.Bounds;
            }

            return rectangle;
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.intersects
        public override bool intersects(Rectangle boundingBox)
        {
            if (this.Model != null)
            {
                Rectangle rectangle = new Rectangle((int)this.tileX.Value * 64, (int)this.tileY.Value * 64, (int)this.tilesWide.Value * 64, (int)this.tilesHigh.Value * 64);
                int additionalTilePropertyRadius = this.GetAdditionalTilePropertyRadius();
                if (additionalTilePropertyRadius > 0)
                {
                    rectangle.Inflate(additionalTilePropertyRadius * 64, additionalTilePropertyRadius * 64);
                }

                var isEntityWithinBuilding = rectangle.Contains(Game1.player.GetBoundingBox());
                if (rectangle.Intersects(boundingBox))
                {
                    for (int i = boundingBox.Left / 64; i <= boundingBox.Right / 64; i++)
                    {
                        for (int j = boundingBox.Top / 64; j <= boundingBox.Bottom / 64; j++)
                        {
                            if (!this.isTilePassable(new Vector2(i, j)))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            return new Rectangle((int)this.tileX.Value * 64, (int)this.tileY.Value * 64, (int)this.tilesWide.Value * 64, (int)this.tilesHigh.Value * 64).Intersects(boundingBox);
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.isTilePassable
        public override bool isTilePassable(Vector2 tile)
        {
            bool flag = this.occupiesTile(tile);
            if (flag && this.isUnderConstruction())
            {
                return false;
            }
            if (this.Model != null && this.IsInTilePropertyRadius(tile))
            {
                return this.Model.IsTilePassable((int)tile.X - this.tileX.Value, (int)tile.Y - this.tileY.Value);
            }
            return !flag;
        }


        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.IsInTilePropertyRadius
        public virtual bool IsInTilePropertyRadius(Vector2 tileLocation)
        {
            int additionalTilePropertyRadius = this.GetAdditionalTilePropertyRadius();
            if (tileLocation.X >= (float)((int)this.tileX.Value - additionalTilePropertyRadius) && tileLocation.X < (float)((int)this.tileX.Value + (int)this.tilesWide.Value + additionalTilePropertyRadius) && tileLocation.Y >= (float)((int)this.tileY.Value - additionalTilePropertyRadius))
            {
                return tileLocation.Y < (float)((int)this.tileY.Value + (int)this.tilesHigh.Value + additionalTilePropertyRadius);
            }
            return false;
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.getIndoors
        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            GameLocation gameLocation = null;
            if (this.Model != null && !string.IsNullOrEmpty(this.Model.IndoorMap))
            {
                Type type = typeof(GameLocation);
                try
                {
                    if (this.Model.IndoorMapType != null)
                    {
                        type = Type.GetType(this.Model.IndoorMapType);
                    }
                }
                catch (Exception)
                {
                    type = typeof(GameLocation);
                }
                try
                {
                    gameLocation = (GameLocation)Activator.CreateInstance(type, "Maps\\" + this.Model.IndoorMap, this.buildingType.Value);
                }
                catch (Exception)
                {
                    try
                    {
                        gameLocation = (GameLocation)Activator.CreateInstance(type, "Maps\\" + this.Model.IndoorMap);
                    }
                    catch (Exception arg)
                    {
                        SolidFoundations.monitor.Log($"Error trying to instantiate indoors for {this.buildingType}: {arg}", LogLevel.Warn);
                        gameLocation = new GameLocation("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
                    }
                }
            }

            if (gameLocation != null)
            {
                gameLocation.uniqueName.Value = nameOfIndoorsWithoutUnique + Guid.NewGuid().ToString();
                gameLocation.IsFarm = true;
                gameLocation.isStructure.Value = true;
                this.updateInteriorWarps(gameLocation);
            }

            return gameLocation;
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.GetBuildingChest
        public Chest GetBuildingChest(string name)
        {
            foreach (Chest buildingChest in this.buildingChests)
            {
                if (buildingChest.Name == name)
                {
                    return buildingChest;
                }
            }
            return null;
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.GetBuildingChestData
        public BuildingChest GetBuildingChestData(string name)
        {
            if (this.Model == null)
            {
                return null;
            }

            foreach (BuildingChest chest in this.Model.Chests)
            {
                if (chest.Name == name)
                {
                    return chest;
                }
            }
            return null;
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.ShouldDrawShadow
        public bool ShouldDrawShadow()
        {
            if (this.Model != null && !this.Model.DrawShadow)
            {
                return false;
            }
            return true;
        }


        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.draw
        public override void draw(SpriteBatch b)
        {
            if (this.isMoving)
            {
                return;
            }
            if ((int)this.daysOfConstructionLeft.Value > 0 || (int)this.newConstructionTimer.Value > 0)
            {
                this.drawInConstruction(b);
                return;
            }
            if (this.ShouldDrawShadow())
            {
                this.drawShadow(b);
            }
            float num = ((int)this.tileY.Value + (int)this.tilesHigh.Value) * 64;
            float num2 = num;
            if (this.Model != null)
            {
                num2 -= this.Model.SortTileOffset * 64f;
            }
            num2 /= 10000f;
            Vector2 vector = new Vector2((int)this.tileX.Value * 64, (int)this.tileY.Value * 64 + (int)this.tilesHigh.Value * 64);
            Vector2 vector2 = Vector2.Zero;
            if (this.Model != null)
            {
                vector2 = this.Model.DrawOffset * 4f;
            }
            Vector2 vector3 = new Vector2(0f, this.getSourceRect().Height);
            b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, vector + vector2), this.getSourceRect(), this.color.Value * this.alpha.Value, 0f, vector3, 4f, SpriteEffects.None, num2);
            if ((bool)this.magical.Value && this.buildingType.Value.Equals("Gold Clock"))
            {
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.tileX.Value * 64 + 92, (int)this.tileY.Value * 64 - 40)), Town.hourHandSource, Color.White * this.alpha.Value, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / 7000f / 23f)), new Vector2(2.5f, 8f), 3f, SpriteEffects.None, (float)(((int)this.tileY.Value + (int)this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.tileX.Value * 64 + 92, (int)this.tileY.Value * 64 - 40)), Town.minuteHandSource, Color.White * this.alpha.Value, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / 7000f * 1.02f)), new Vector2(2.5f, 12f), 3f, SpriteEffects.None, (float)(((int)this.tileY.Value + (int)this.tilesHigh.Value) * 64) / 10000f + 0.00011f);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.tileX.Value * 64 + 92, (int)this.tileY.Value * 64 - 40)), Town.clockNub, Color.White * this.alpha.Value, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, (float)(((int)this.tileY.Value + (int)this.tilesHigh.Value) * 64) / 10000f + 0.00012f);
            }
            if (this.Model != null)
            {
                foreach (Chest buildingChest2 in this.buildingChests)
                {
                    BuildingChest buildingChestData = this.GetBuildingChestData(buildingChest2.Name);
                    if (buildingChestData.DisplayTile.X != -1f && buildingChestData.DisplayTile.Y != -1f && buildingChest2.items.Count > 0 && buildingChest2.items[0] != null)
                    {
                        num2 = ((float)(int)this.tileY.Value + buildingChestData.DisplayTile.Y + 1f) * 64f;
                        num2 += 1f;
                        float num3 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) - buildingChestData.DisplayHeight * 64f;
                        float num4 = ((float)(int)this.tileX.Value + buildingChestData.DisplayTile.X) * 64f;
                        float num5 = ((float)(int)this.tileY.Value + buildingChestData.DisplayTile.Y - 1f) * 64f;
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(num4, num5 + num3)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2 / 10000f);

                        // TODO: Display item texture here
                        //ParsedItemData itemDataForItemID = Utility.GetItemDataForItemID(buildingChest2.items[0].QualifiedItemID);
                        //b.Draw(itemDataForItemID.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(num4 + 32f + 4f, num5 + 32f + num3)), itemDataForItemID.GetSourceRect(0), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (num2 + 1f) / 10000f);
                    }
                }
                if (this.Model.DrawLayers != null)
                {
                    foreach (BuildingDrawLayer drawLayer in this.Model.DrawLayers)
                    {
                        if (drawLayer.DrawInBackground)
                        {
                            continue;
                        }
                        if (drawLayer.OnlyDrawIfChestHasContents != null)
                        {
                            Chest buildingChest = this.GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
                            if (buildingChest == null || buildingChest.isEmpty())
                            {
                                continue;
                            }
                        }
                        num2 = num - drawLayer.SortTileOffset * 64f;
                        num2 += 1f;
                        num2 /= 10000f;
                        Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
                        sourceRect = this.ApplySourceRectOffsets(sourceRect);
                        vector2 = Vector2.Zero;
                        if (drawLayer.AnimalDoorOffset != Point.Zero)
                        {
                            vector2 = new Vector2((float)drawLayer.AnimalDoorOffset.X * this.animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * this.animalDoorOpenAmount.Value);
                        }
                        Texture2D texture2D = this.texture.Value;
                        if (drawLayer.Texture != null)
                        {
                            texture2D = Game1.content.Load<Texture2D>(drawLayer.Texture);
                        }
                        b.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, vector + (vector2 - vector3 + drawLayer.DrawPosition) * 4f), sourceRect, this.color.Value * this.alpha.Value, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, num2);
                    }
                }
            }
            if ((int)this.daysUntilUpgrade.Value <= 0)
            {
                return;
            }
            if (this.Model != null)
            {
                if (this.Model.UpgradeSignTile.X >= 0f)
                {
                    num2 = ((float)(int)this.tileY.Value + this.Model.UpgradeSignTile.Y + 1f) * 64f;
                    num2 += 2f;
                    num2 /= 10000f;
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Rectangle(367, 309, 16, 15), Color.White * this.alpha.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2);
                }
            }
            else if (this.indoors.Value != null && this.indoors.Value is Shed)
            {
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Rectangle(367, 309, 16, 15), Color.White * this.alpha.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)this.tileY.Value + (int)this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
            }
        }

        // TODO: When updated to SDV v1.6, this class should be deleted in favor of using the native StardewValley.Buildings.Building.draw
        public override void drawBackground(SpriteBatch b)
        {
            if (this.isMoving || (int)this.daysOfConstructionLeft > 0 || (int)this.newConstructionTimer > 0 || this.Model == null || this.Model.DrawLayers == null)
            {
                return;
            }

            Vector2 vector = new Vector2((int)this.tileX * 64, (int)this.tileY * 64 + (int)this.tilesHigh * 64);
            Vector2 vector2 = new Vector2(0f, this.getSourceRect().Height);
            foreach (BuildingDrawLayer drawLayer in this.Model.DrawLayers)
            {
                if (!drawLayer.DrawInBackground)
                {
                    continue;
                }

                if (drawLayer.OnlyDrawIfChestHasContents != null)
                {
                    Chest buildingChest = this.GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
                    if (buildingChest == null || buildingChest.isEmpty())
                    {
                        continue;
                    }
                }
                Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
                sourceRect = this.ApplySourceRectOffsets(sourceRect);
                Vector2 vector3 = Vector2.Zero;
                if (drawLayer.AnimalDoorOffset != Point.Zero)
                {
                    vector3 = new Vector2((float)drawLayer.AnimalDoorOffset.X * this.animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * this.animalDoorOpenAmount.Value);
                }
                Texture2D texture2D = this.texture.Value;
                if (drawLayer.Texture != null)
                {
                    texture2D = Game1.content.Load<Texture2D>(drawLayer.Texture);
                }
                b.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, vector + (vector3 - vector2 + drawLayer.DrawPosition) * 4f), sourceRect, this.color.Value * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0f);
            }
        }

    }
}
