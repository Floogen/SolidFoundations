using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using SolidFoundations.Framework.Utilities;
using StardewModdingAPI;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Compatibility
{
    public class OldExtendedBuildingModel : ExtendedBuildingModel
    {
        public new string SourceRect { set { base.SourceRect = Toolkit.GetRectangleFromString(value); } }
        public new string AnimalDoor { set { base.AnimalDoor = Toolkit.GetRectangleFromString(value); } }

        public new List<OldExtendedBuildingDrawLayer> DrawLayers;
    }
}
