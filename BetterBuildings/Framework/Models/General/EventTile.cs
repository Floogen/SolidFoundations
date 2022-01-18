using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class EventTile
    {
        public WarpEvent Warp { get; set; }
        public FadeEvent Fade { get; set; }
        public bool DrawOverPlayer { get; set; }
        public TileLocation Tile { get; set; }


        public void Trigger(GenericBuilding customBuilding, Farmer who)
        {
            customBuilding.DrawOverPlayer = DrawOverPlayer;

            if (Warp is not null)
            {
                Game1.warpFarmer(Warp.Map, Warp.DestinationTile.X, Warp.DestinationTile.Y, Game1.player.FacingDirection);
            }
            if (Fade is not null)
            {
                customBuilding.AlphaOverride = Math.Min(Fade.Percentage, 1f);
            }

            customBuilding.IsUsingEventOverride = true;
        }
    }
}
