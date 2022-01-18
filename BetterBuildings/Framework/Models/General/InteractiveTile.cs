using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
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
        OpenShop
    }

    public class InteractiveTile
    {
        public ShopOpenEvent ShopOpen { get; set; }
        public TileLocation Tile { get; set; }

        public void Trigger(GenericBuilding customBuilding, Farmer who)
        {
            if (ShopOpen is not null)
            {
                Game1.activeClickableMenu = new ShopMenu((Game1.getLocationFromName("Sewer") as Sewer).getShadowShopStock(), 0, "KrobusGone", null);
            }
        }
    }
}
