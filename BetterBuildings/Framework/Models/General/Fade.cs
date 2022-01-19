using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class Fade
    {
        public bool Enabled { get; set; } = true;
        public int MinTileHeightBeforeFade { get; set; } = -1;
        private float _fadeAmount { get; set; } = -1f;
        public float AmountToFade { get { return _fadeAmount; } set { _fadeAmount = Math.Max(0f, Math.Min(value, 1f)); } }
    }
}
