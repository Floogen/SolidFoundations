using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
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
        public BuildingExtended Model { get; set; }
        public string Id { get; set; }
        public string LocationName { get; set; }

        private Texture2D _lavaTexture;

        public GenericBuilding() : base()
        {

        }

        public GenericBuilding(BuildingExtended model) : base(new BluePrint(model.ID), Vector2.Zero)
        {
            RefreshModel(model);
        }

        public GenericBuilding(BuildingExtended model, BluePrint bluePrint) : base(bluePrint, Vector2.Zero)
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

        public void RefreshModel(BuildingExtended model)
        {
            Model = model;
            Id = model.ID;

            base.tilesHigh.Value = model.Size.X;
            base.tilesWide.Value = model.Size.Y;
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
    }
}
