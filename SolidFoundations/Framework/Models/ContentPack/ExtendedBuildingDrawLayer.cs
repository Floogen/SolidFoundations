using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Utilities.Backport;
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
        public string Condition { get; set; }
        public string[] ModDataFlags { get; set; }

        private int _cachedTime;
        private int _elapsedTime;
        private int _currentSequenceIndex;

        public Rectangle GetSourceRect(int time, GenericBuilding building)
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
                _currentSequenceIndex = GetNextValidFrame(building, _currentSequenceIndex);
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

        public int GetNextValidFrame(GenericBuilding building, int startingValue = 0)
        {
            var currentIndex = startingValue;
            if (currentIndex + 1 < Sequences.Count)
            {
                currentIndex = currentIndex + 1;
            }
            else
            {
                currentIndex = 0;
            }

            var sequence = Sequences[currentIndex];
            if (building.ValidateConditions(sequence.Condition, sequence.ModDataFlags) is false)
            {
                currentIndex = GetNextValidFrame(building, currentIndex);
            }
            return currentIndex;
        }
    }
}
