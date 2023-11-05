using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingActionTiles : BuildingActionTile
    {
        public SpecialAction SpecialAction { get; set; }
    }
}
