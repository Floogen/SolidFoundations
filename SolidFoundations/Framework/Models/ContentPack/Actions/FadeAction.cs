using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class FadeAction
    {
        public int DurationInSeconds { get; set; }
        public float Speed { get; set; } = 0.02f;
        public SpecialAction ActionAfterFade { get; set; }
    }
}
