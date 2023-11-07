using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingSkin : BuildingSkin
    {
        public string ID { get { return base.Id; } set { base.Id = value; } }

        [ContentSerializer(Optional = true)]
        public List<PaintMaskData> PaintMasks;

        [ContentSerializer(Optional = true)]
        public string PaintMaskTexture;
    }
}
