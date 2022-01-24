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
        public List<TextureAnimation> AnimationOverride { get; set; }
        public List<Condition> Conditions { get; set; }
        public bool DrawOverPlayer { get; set; }
        public bool StopAtFinish { get; set; }
        internal Color ActualColor { get; set; } = Microsoft.Xna.Framework.Color.White;
        public int[] Color { set { ActualColor = GetColor(value); } }

        internal EffectModel Model { get; set; }

        private int _animationTimer { get; set; }
        private int _animationIndex { get; set; }
        private int _animationStartingIndex { get; set; }


        public List<TextureAnimation> GetAnimations()
        {
            if (AnimationOverride is null || AnimationOverride.Count == 0)
            {
                return Model.Animation;
            }

            return AnimationOverride;
        }

        public Rectangle GetSourceRectangle()
        {
            int x = 0;
            int y = 0;
            int width = Model.TextureDimensions.Width * 16;
            int height = Model.TextureDimensions.Height * 16;

            if (GetAnimations().Count > 0)
            {
                x = GetAnimations()[_animationIndex].Frame * width;
                y = GetAnimations()[_animationIndex].RowOffset * height;
            }

            return new Rectangle(x, y, width, height);
        }

        public void UpdateTimer(GenericBuilding customBuilding, int milliseconds)
        {
            _animationTimer -= milliseconds;

            if (GetAnimations().Count > 0 && _animationTimer <= 0)
            {
                UpdateAnimationIndex(customBuilding);
                _animationTimer = GetAnimations()[_animationIndex].Duration;
            }
        }

        public void Reset()
        {
            _animationIndex = 0;
            _animationStartingIndex = -1;
            _animationTimer = GetAnimations()[_animationIndex].Duration;
        }

        public void UpdateAnimationIndex(GenericBuilding customBuilding, bool hasDoneFullLoop = false)
        {
            _animationIndex = GetAnimations().Count <= _animationIndex + 1 ? _animationStartingIndex : _animationIndex + 1;
            if (_animationStartingIndex == -1)
            {
                _animationStartingIndex = 0;
            }

            var passedConditions = GetAnimations()[_animationIndex].PassesAllConditions(customBuilding);
            if (GetAnimations()[_animationIndex].OverrideStartingIndex)
            {
                _animationStartingIndex = _animationIndex;
            }
            _animationStartingIndex = passedConditions is false && _animationIndex == _animationStartingIndex ? 0 : _animationStartingIndex;

            if (!passedConditions && (_animationIndex > _animationStartingIndex || hasDoneFullLoop is false))
            {
                if (_animationIndex == _animationStartingIndex)
                {
                    hasDoneFullLoop = true;
                }

                UpdateAnimationIndex(customBuilding, hasDoneFullLoop);
            }
        }

        public bool PassesAllConditions(GenericBuilding customBuilding)
        {
            if (Conditions is null)
            {
                return true;
            }

            foreach (var condition in Conditions)
            {
                if (!condition.IsValid(customBuilding))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasFinished()
        {
            if (StopAtFinish && _animationIndex == _animationStartingIndex)
            {
                return true;
            }

            return false;
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
