using StardewValley.GameData.Buildings;
using System.Collections.Generic;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingSkin : BuildingSkin
    {
        public string ID { get { return base.Id; } set { base.Id = value; } }

        public List<PaintMaskData> PaintMasks;

        public string PaintMaskTexture;
    }
}
