using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Utilities;
using SolidFoundations.Framework.Utilities.Backport;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
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
        [XmlIgnore]
        public ExtendedBuildingModel Model { get; set; }
        public string Id { get; set; }
        public string LocationName { get; set; }

        // Start of backported properties
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.buildingLocation
        [XmlIgnore]
        public NetLocationRef buildingLocation = new NetLocationRef();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.buildingChests
        public NetList<Chest, NetRef<Chest>> buildingChests = new NetList<Chest, NetRef<Chest>>();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.animalDoorOpenAmount
        public readonly NetFloat animalDoorOpenAmount = new NetFloat();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._buildingMetadata
        [XmlIgnore]
        protected Dictionary<string, string> _buildingMetadata = new Dictionary<string, string>();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._lastHouseUpgradeLevel
        protected int _lastHouseUpgradeLevel = -1;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._chimneyPosition
        protected Vector2 _chimneyPosition = Vector2.Zero;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._hasChimney
        protected bool? _hasChimney;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.chimneyTimer
        protected int chimneyTimer = 500;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.skinID
        public NetString skinID = new NetString();

        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.nonInstancedIndoors
        public readonly NetLocationRef nonInstancedIndoors = new NetLocationRef();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.IndoorOrInstancedIndoor
        public GameLocation IndoorOrInstancedIndoor
        {
            get
            {
                if (this.indoors.Value != null)
                {
                    return this.indoors.Value;
                }
                return this.nonInstancedIndoors.Value;
            }
        }

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

        public bool ValidateConditions(string condition, string[] modDataFlags = null)
        {
            if (GameStateQuery.CheckConditions(condition))
            {
                if (modDataFlags is not null)
                {
                    foreach (string flag in modDataFlags)
                    {
                        // Clear whitespace
                        var cleanedFlag = flag.Replace(" ", String.Empty);
                        bool flagShouldNotExist = String.IsNullOrEmpty(cleanedFlag) || cleanedFlag[0] != '!' ? false : true;
                        if (flagShouldNotExist)
                        {
                            cleanedFlag = cleanedFlag[1..];
                        }
                        cleanedFlag = String.Concat(ModDataKeys.FLAG_BASE, ".", cleanedFlag.ToLower());

                        //string flagKey = cleanedFlag.Contains(':') ? cleanedFlag.Split(':')[0] : String.Empty;
                        //string flagValue = cleanedFlag.Contains(':') && cleanedFlag.Split(':').Length > 1 ? cleanedFlag.Split(':')[1] : String.Empty;

                        if (this.modData.ContainsKey(cleanedFlag) is false == flagShouldNotExist is false)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.OnUseHumanDoor
        public virtual void ToggleAnimalDoor(Farmer who)
        {
            if (!this.animalDoorOpen)
            {
                if (this.Model.AnimalDoorOpenSound != null)
                {
                    who.currentLocation.playSound(this.Model.AnimalDoorOpenSound);
                }
            }
            else if (this.Model.AnimalDoorCloseSound != null)
            {
                who.currentLocation.playSound(this.Model.AnimalDoorCloseSound);
            }
            this.animalDoorOpen.Value = !this.animalDoorOpen;
        }

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.OnUseHumanDoor
        public virtual bool OnUseHumanDoor(Farmer who)
        {
            return true;
        }

        // Preserve this override (specifically the CustomAction) when updated to SDV v1.6, but call the base doAction method if ExtendedBuildingModel.
        public override bool doAction(Vector2 tileLocation, Farmer who)
        {
            if (who.isRidingHorse())
            {
                return false;
            }
            if (who.IsLocalPlayer && tileLocation.X >= (float)(int)this.tileX && tileLocation.X < (float)((int)this.tileX + (int)this.tilesWide) && tileLocation.Y >= (float)(int)this.tileY && tileLocation.Y < (float)((int)this.tileY + (int)this.tilesHigh) && (int)this.daysOfConstructionLeft > 0)
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
            }
            else
            {
                if (who.IsLocalPlayer && tileLocation.X == (float)(this.humanDoor.X + (int)this.tileX) && tileLocation.Y == (float)(this.humanDoor.Y + (int)this.tileY) && this.IndoorOrInstancedIndoor != null)
                {
                    if (who.mount != null)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
                        return false;
                    }
                    if (who.team.demolishLock.IsLocked())
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
                        return false;
                    }
                    if (this.OnUseHumanDoor(who))
                    {
                        who.currentLocation.playSoundAt("doorClose", tileLocation);
                        bool isStructure = false;
                        if (this.indoors.Value != null)
                        {
                            isStructure = true;
                        }
                        Game1.warpFarmer(this.IndoorOrInstancedIndoor.NameOrUniqueName, this.IndoorOrInstancedIndoor.warps[0].X, this.IndoorOrInstancedIndoor.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
                    }
                    return true;
                }
                if (this.Model != null)
                {
                    Microsoft.Xna.Framework.Rectangle rectForAnimalDoor = this.getRectForAnimalDoor();
                    rectForAnimalDoor.Width /= 64;
                    rectForAnimalDoor.Height /= 64;
                    rectForAnimalDoor.X /= 64;
                    rectForAnimalDoor.Y /= 64;
                    if ((int)this.daysOfConstructionLeft <= 0 && rectForAnimalDoor != Microsoft.Xna.Framework.Rectangle.Empty && rectForAnimalDoor.Contains(Utility.Vector2ToPoint(tileLocation)) && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
                    {
                        this.ToggleAnimalDoor(who);
                        return true;
                    }
                    this.GetAdditionalTilePropertyRadius();
                    if (who.IsLocalPlayer && this.IsInTilePropertyRadius(tileLocation) && !this.isTilePassable(tileLocation))
                    {
                        string actionAtTile = this.Model.GetActionAtTile((int)tileLocation.X - this.tileX.Value, (int)tileLocation.Y - this.tileY.Value);
                        if (actionAtTile != null)
                        {
                            actionAtTile = TextParser.ParseText(actionAtTile);
                            if (who.currentLocation.performAction(actionAtTile, who, new xTile.Dimensions.Location((int)tileLocation.X, (int)tileLocation.Y)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return base.doAction(tileLocation, who);
        }

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.doesTileHaveProperty
        public override bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
        {
            if (this.Model != null)
            {
                return this.Model.HasPropertyAtTile(tile_x - this.tileX.Value, tile_y - this.tileY.Value, property_name, layer_name, ref property_value);
            }
            return false;
        }


        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.ApplySourceRectOffsets
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.getSourceRect
        public override Rectangle getSourceRect()
        {
            Rectangle rectangle = this.Model.GetSourceRect();
            if (rectangle == Rectangle.Empty)
            {
                return base.texture.Value.Bounds;
            }

            return rectangle;
        }

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.intersects
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.isTilePassable
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


        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.IsInTilePropertyRadius
        public virtual bool IsInTilePropertyRadius(Vector2 tileLocation)
        {
            int additionalTilePropertyRadius = this.GetAdditionalTilePropertyRadius();
            if (tileLocation.X >= (float)((int)this.tileX.Value - additionalTilePropertyRadius) && tileLocation.X < (float)((int)this.tileX.Value + (int)this.tilesWide.Value + additionalTilePropertyRadius) && tileLocation.Y >= (float)((int)this.tileY.Value - additionalTilePropertyRadius))
            {
                return tileLocation.Y < (float)((int)this.tileY.Value + (int)this.tilesHigh.Value + additionalTilePropertyRadius);
            }
            return false;
        }

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.getIndoors
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.GetBuildingChest
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.GetBuildingChestData
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.ShouldDrawShadow
        public bool ShouldDrawShadow()
        {
            if (this.Model != null && !this.Model.DrawShadow)
            {
                return false;
            }
            return true;
        }

        // Preserve this override when updated to SDV v1.6, but call the base draw method if ExtendedBuildingModel.
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
            if (this.Model is null || this.Model.DrawLayers is null || this.Model.DrawLayers.Any(l => l is not null && l.HideBaseTexture && ValidateConditions(l.Condition, l.ModDataFlags)) is false)
            {
                b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, vector + vector2), this.getSourceRect(), this.color.Value * this.alpha.Value, 0f, vector3, 4f, SpriteEffects.None, num2);
            }
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
                    foreach (ExtendedBuildingDrawLayer drawLayer in this.Model.DrawLayers.Where(d => ValidateConditions(d.Condition, d.ModDataFlags)))
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
                        Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds, this);
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.draw
        public override void drawBackground(SpriteBatch b)
        {
            if (this.isMoving || (int)this.daysOfConstructionLeft > 0 || (int)this.newConstructionTimer > 0 || this.Model == null || this.Model.DrawLayers == null)
            {
                return;
            }

            Vector2 vector = new Vector2((int)this.tileX * 64, (int)this.tileY * 64 + (int)this.tilesHigh * 64);
            Vector2 vector2 = new Vector2(0f, this.getSourceRect().Height);
            foreach (ExtendedBuildingDrawLayer drawLayer in this.Model.DrawLayers)
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
                Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds, this);
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

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.Buildings.Building.GetMetadata
        public string GetMetadata(string key)
        {
            if (this._buildingMetadata == null)
            {
                this._buildingMetadata = new Dictionary<string, string>();
                if (this.Model != null)
                {
                    foreach (KeyValuePair<string, string> metadatum in this.Model.Metadata)
                    {
                        this._buildingMetadata[metadatum.Key] = metadatum.Value;
                    }
                    if (this.Model.Skins != null && this.Model.Skins.Count > 0 && this.skinID.Value != null)
                    {
                        foreach (BuildingSkin skin in this.Model.Skins)
                        {
                            if (!(skin.ID == this.skinID.Value))
                            {
                                continue;
                            }
                            foreach (KeyValuePair<string, string> metadatum2 in skin.Metadata)
                            {
                                this._buildingMetadata[metadatum2.Key] = metadatum2.Value;
                            }
                            break;
                        }
                    }
                }
            }
            if (this._buildingMetadata.TryGetValue(key, out key))
            {
                return key;
            }
            return null;
        }

        // TODO: When updated to SDV v1.6, this method should be deleted in favor of using the native StardewValley.GameLocation.hasActiveFireplace
        public bool HasActiveFireplaceBackport()
        {
            if (this.Model is null || this.IndoorOrInstancedIndoor is null)
            {
                return false;
            }

            for (int i = 0; i < this.IndoorOrInstancedIndoor.furniture.Count(); i++)
            {
                if ((int)this.IndoorOrInstancedIndoor.furniture[i].furniture_type.Value == 14 && (bool)this.IndoorOrInstancedIndoor.furniture[i].isOn.Value)
                {
                    return true;
                }
            }
            return false;
        }


        // TODO: When updated to SDV v1.6, this method should be deleted
        private void UpdateBackport(GameTime time)
        {
            this.alpha.Value = Math.Min(1f, this.alpha.Value + 0.05f);
            int num = this.tilesHigh.Get();
            if (this.fadeWhenPlayerIsBehind.Value && Game1.player.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(64 * (int)this.tileX, 64 * ((int)this.tileY + (-(this.getSourceRectForMenu().Height / 16) + num)), (int)this.tilesWide * 64, (this.getSourceRectForMenu().Height / 16 - num) * 64 + 32)))
            {
                this.alpha.Value = Math.Max(0.4f, this.alpha.Value - 0.09f);
            }
            if (this.isUnderConstruction())
            {
                return;
            }
            if (!this._hasChimney.HasValue)
            {
                string metadata = this.GetMetadata("ChimneyPosition");
                if (metadata != null)
                {
                    this._hasChimney = true;
                    string[] array = metadata.Split(' ');
                    this._chimneyPosition.X = int.Parse(array[0]);
                    this._chimneyPosition.Y = int.Parse(array[1]);
                }
                else
                {
                    this._hasChimney = false;
                }
            }
            if (this.IndoorOrInstancedIndoor is FarmHouse)
            {
                int houseUpgradeLevel = (this.IndoorOrInstancedIndoor as FarmHouse).owner.HouseUpgradeLevel;
                if (this._lastHouseUpgradeLevel != houseUpgradeLevel)
                {
                    this._lastHouseUpgradeLevel = houseUpgradeLevel;
                    string text = null;
                    for (int i = 1; i <= this._lastHouseUpgradeLevel; i++)
                    {
                        string metadata2 = this.GetMetadata("ChimneyPosition" + (i + 1));
                        if (metadata2 != null)
                        {
                            text = metadata2;
                        }
                    }
                    if (text != null)
                    {
                        this._hasChimney = true;
                        string[] array2 = text.Split(' ');
                        this._chimneyPosition.X = int.Parse(array2[0]);
                        this._chimneyPosition.Y = int.Parse(array2[1]);
                    }
                }
            }
            if (!this._hasChimney.Value)
            {
                return;
            }
            this.chimneyTimer -= time.ElapsedGameTime.Milliseconds;
            if (this.chimneyTimer > 0)
            {
                return;
            }
            if (this.HasActiveFireplaceBackport())
            {
                GameLocation value = this.buildingLocation.Value;
                Vector2 vector = new Vector2((int)this.tileX * 64, (int)this.tileY * 64 + num * 64 - this.getSourceRect().Height * 4);
                Vector2 vector2 = Vector2.Zero;
                if (this.Model != null)
                {
                    vector2 = this.Model.DrawOffset * 4f;
                }
                TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(vector.X + vector2.X, vector.Y + vector2.Y) + this._chimneyPosition * 4f + new Vector2(-8f, -12f), flipped: false, 0.002f, Color.Gray);
                temporaryAnimatedSprite.alpha = 0.75f;
                temporaryAnimatedSprite.motion = new Vector2(0f, -0.5f);
                temporaryAnimatedSprite.acceleration = new Vector2(0.002f, 0f);
                temporaryAnimatedSprite.interval = 99999f;
                temporaryAnimatedSprite.layerDepth = 1f;
                temporaryAnimatedSprite.scale = 2f;
                temporaryAnimatedSprite.scaleChange = 0.02f;
                temporaryAnimatedSprite.rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f;
                value.temporarySprites.Add(temporaryAnimatedSprite);
            }
            this.chimneyTimer = 500;
        }

        public override void Update(GameTime time)
        {
            // TODO: When updated to SDV v1.6, this line should be replaced with base.Update(time)
            UpdateBackport(time);

            // Catch touch actions
            if (this.Model != null)
            {
                Vector2 playerStandingPosition = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
                if (buildingLocation.Value.lastTouchActionLocation.Equals(Vector2.Zero))
                {
                    string eventTile = this.Model.GetEventAtTile((int)playerStandingPosition.X - this.tileX.Value, (int)playerStandingPosition.Y - this.tileY.Value);
                    buildingLocation.Value.lastTouchActionLocation = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
                    if (eventTile != null)
                    {
                        eventTile = TextParser.ParseText(eventTile);
                        eventTile = SolidFoundations.modHelper.Reflection.GetMethod(new Dialogue(eventTile, null), "checkForSpecialCharacters").Invoke<string>(eventTile);
                        if (buildingLocation.Value.performAction(eventTile, Game1.player, new xTile.Dimensions.Location((int)buildingLocation.Value.lastTouchActionLocation.X, (int)buildingLocation.Value.lastTouchActionLocation.Y)) is false)
                        {
                            buildingLocation.Value.performTouchAction(eventTile, playerStandingPosition);
                        }
                    }
                }
                else if (!buildingLocation.Value.lastTouchActionLocation.Equals(playerStandingPosition))
                {
                    buildingLocation.Value.lastTouchActionLocation = Vector2.Zero;
                }
            }
        }
    }
}
