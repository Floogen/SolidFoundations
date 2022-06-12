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
    public class ExtendedBuildingSkin : BuildingSkin
    {
        [ContentSerializer(Optional = true)]
        public List<PaintMaskData> PaintMasks;

        [ContentSerializer(Optional = true)]
        public string PaintMaskTexture;
    }
}
