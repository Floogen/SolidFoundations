using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.General;
using BetterBuildings.Framework.Models.General.Tiles;
using BetterBuildings.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Models.Effects
{
    public class GenericEffect
    {
        public string Name { get; set; }
        public TileLocation Tile { get; set; } = new TileLocation();
        public Dimensions OffsetInPixels { get; set; } = new Dimensions();
        internal Color ActualColor { get; set; } = Microsoft.Xna.Framework.Color.White;
        public int[] Color { set { ActualColor = GetColor(value); } }

        internal EffectModel Model { get; set; }

        private int _animationTimer { get; set; }
        private int _animationIndex { get; set; }


        public void UpdateTimer(int milliseconds)
        {
            _animationTimer -= milliseconds;

            if (Model.Animation.Count > 0 && _animationTimer <= 0)
            {
                _animationIndex = Model.Animation.Count <= _animationIndex + 1 ? 0 : _animationIndex + 1;
                _animationTimer = Model.Animation[_animationIndex].Duration;
            }
        }

        public Rectangle GetSourceRectangle()
        {
            int x = 0;
            int y = 0;
            int width = Model.TextureDimensions.Width * 16;
            int height = Model.TextureDimensions.Height * 16;

            if (Model.Animation.Count > 0)
            {
                x = Model.Animation[_animationIndex].Frame * width;
                y = Model.Animation[_animationIndex].RowOffset * height;
            }

            return new Rectangle(x, y, width, height);
        }

        private int GetColorIndex(int[] colorArray, int position)
        {
            if (position >= colorArray.Length)
            {
                return 255;
            }

            return colorArray[position];
        }

        private Color GetColor(int[] colorArray)
        {
            return new Color(GetColorIndex(colorArray, 0), GetColorIndex(colorArray, 1), GetColorIndex(colorArray, 2), GetColorIndex(colorArray, 3));
        }
    }
}
