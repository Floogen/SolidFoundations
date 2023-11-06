using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Extensions;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using SolidFoundations.Framework.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class OldExtendedBuildingDrawLayer : ExtendedBuildingDrawLayer
    {
        public new string SourceRect { set { base.SourceRect = Toolkit.GetRectangleFromString(value); } }
    }
}
