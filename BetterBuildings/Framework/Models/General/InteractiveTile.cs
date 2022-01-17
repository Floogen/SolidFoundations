using BetterBuildings.Framework.Models.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public enum InteractiveType
    {
        Input,
        Output,
        Warp,
        Message,
        PlaySound,
        OpenShop,
        //FadeBuilding
    }

    public class InteractiveTile
    {
        public WarpEvent Warp { get; set; }
        public TileLocation Tile { get; set; }


        public void Trigger(Farmer who)
        {
            if (Warp is not null)
            {
                Game1.warpFarmer(Warp.Map, Warp.DestinationTile.X, Warp.DestinationTile.Y, Game1.player.FacingDirection);
            }
        }
    }
}
