using SolidFoundations.Framework.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SolidFoundations.Framework.Models.ContentPack.Compatibility
{
    public class OldExtendedBuildingModel : ExtendedBuildingModel
    {
        public new string SourceRect { set { base.SourceRect = Toolkit.GetRectangleFromString(value); } }
        public new string AnimalDoor { set { base.AnimalDoor = Toolkit.GetRectangleFromString(value); } }

        public new List<OldExtendedBuildingDrawLayer> DrawLayers
        {
            set
            {
                var layers = value;
                layers.ForEach(l => (l as ExtendedBuildingDrawLayer).SourceRect = Toolkit.GetRectangleFromString(l.SourceRect));

                base.DrawLayers = layers.ToList<ExtendedBuildingDrawLayer>();
            }
        }
    }
}
