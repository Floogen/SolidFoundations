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
        public List<TextureAnimation> Sequences { get; set; } = new List<TextureAnimation> { };
        public List<GenericEffect> Effects { get; set; } = new List<GenericEffect>();
    }
}
