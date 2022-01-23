using BetterBuildings.Framework.Models.Effects;
using BetterBuildings.Framework.Models.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public class AnimationModel
    {
        public List<TextureAnimation> Sequences { get; set; } = new List<TextureAnimation>();
        public List<GenericEffect> Effects { get; set; } = new List<GenericEffect>();

        private int _animationTimer { get; set; }
        private int _animationIndex { get; set; }
        private int _animationStartingIndex { get; set; }


        public TextureAnimation GetCurrentTextureAnimation()
        {
            return Sequences[_animationIndex];
        }

        public void UpdateTimer(GenericBuilding customBuilding, int milliseconds)
        {
            _animationTimer -= milliseconds;

            if (Sequences.Count > 0 && _animationTimer <= 0)
            {
                UpdateAnimationIndex(customBuilding);
                _animationTimer = Sequences[_animationIndex].Duration;
            }
        }

        public void UpdateAnimationIndex(GenericBuilding customBuilding, bool hasDoneFullLoop = false)
        {
            _animationIndex = Sequences.Count <= _animationIndex + 1 ? _animationStartingIndex : _animationIndex + 1;

            var passedConditions = Sequences[_animationIndex].PassesAllConditions(customBuilding);
            if (Sequences[_animationIndex].OverrideStartingIndex)
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

        public void Reset()
        {
            _animationIndex = 0;
            _animationTimer = 0;

            foreach (var effect in Effects)
            {
                effect.Reset();
            }
        }
    }
}
