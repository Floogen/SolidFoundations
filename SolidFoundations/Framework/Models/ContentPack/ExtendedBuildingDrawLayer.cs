using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{

    // TODO: When using SDV v1.6, this class should inherit StardewValley.GameData.BuildingDrawLayer
    public class ExtendedBuildingDrawLayer : BuildingDrawLayer
    {
        public bool HideBaseTexture { get; set; }
        public List<Sequence> Sequences { get; set; }

        private int _cachedTime;

        private int _elapsedTime;

        private int _currentSequenceIndex;

        public new Rectangle GetSourceRect(int time)
        {
            if (Sequences is null || Sequences.Count <= _currentSequenceIndex)
            {
                return base.GetSourceRect(time);
            }
            else if (_cachedTime is default(int))
            {
                _cachedTime = time;
                return base.GetSourceRect();
            }

            var sequence = Sequences[_currentSequenceIndex];
            if (_elapsedTime > sequence.Duration)
            {
                _elapsedTime = 0;
                _currentSequenceIndex = _currentSequenceIndex + 1 >= Sequences.Count ? 0 : _currentSequenceIndex + 1;
            }
            _elapsedTime += time - _cachedTime;
            _cachedTime = time;

            var sourceRect = base.GetSourceRect();
            if (this.FramesPerRow < 0)
            {
                sourceRect.X += sourceRect.Width * sequence.Frame;
            }
            else
            {
                sourceRect.X += sourceRect.Width * (sequence.Frame % this.FramesPerRow);
                sourceRect.Y += sourceRect.Height * (sequence.Frame / this.FramesPerRow);
            }
            return sourceRect;
        }
    }
}
