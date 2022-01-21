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
    public class EffectModel
    {
        public string Name { get; set; }
        public Dimensions TextureDimensions { get; set; }
        public List<TextureAnimation> Animation { get; set; }

        internal string Owner { get; set; }
        internal string PackName { get; set; }
        internal string Id { get; set; }
        internal Texture2D Texture { get; set; }
    }
}
