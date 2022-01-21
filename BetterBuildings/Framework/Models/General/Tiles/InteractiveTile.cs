﻿using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.Events;
using BetterBuildings.Framework.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Models.General.Tiles
{
    public enum InteractiveType
    {
        Input,
        Output,
        Storage, // May be same as output?
        Warp,
        Message,
        PlaySound,
        OpenShop
    }

    public class InteractiveTile : TileBase
    {
        public ShopOpenEvent ShopOpen { get; set; }
        public InputEvent Input { get; set; }
        public DialogueEvent Dialogue { get; set; }
        public MessageEvent Message { get; set; }

        public void Trigger(GenericBuilding customBuilding, Farmer who)
        {
            if (ShopOpen is not null)
            {
                if (ShopOpen.Type is StoreType.Vanilla)
                {
                    HandleVanillaShopMenu(ShopOpen.Name);
                }
                else if (ShopOpen.Type is StoreType.STF && BetterBuildings.apiManager.GetShopTileFrameworkApi() is not null)
                {
                    BetterBuildings.apiManager.GetShopTileFrameworkApi().OpenItemShop(ShopOpen.Name);
                }
            }
            if (Input is not null)
            {
                var requiredItems = InventoryTools.GetActualRequiredItems(Input.RequiredItems);
                if (who.ActiveObject is not null && !InventoryTools.IsHoldingRequiredItem(requiredItems))
                {
                    if (Input.BadInputMessage is not null)
                    {
                        Game1.addHUDMessage(new HUDMessage(Input.BadInputMessage.Text, (int)Input.BadInputMessage.Icon + 1));
                    }
                }
                else if (InventoryTools.IsHoldingRequiredItem(requiredItems) && InventoryTools.HasRequiredItemsInInventory(requiredItems))
                {
                    foreach (Object item in requiredItems)
                    {
                        // Consume the item from the player's inventory
                        InventoryTools.ConsumeItemBasedOnQuantityAndQuality(item, item.Stack, item.Quality);

                        // Add the consumed items to the building's input chest
                        customBuilding.InputStorage.Value.addItem(item);
                    }

                    // Start production if this input has it enabled
                    if (Input.StartProduction)
                    {
                        customBuilding.StartProduction();
                    }
                }
                else
                {
                    var missingItemsDialogue = new DialogueEvent() { Text = "Input requires the following items:^" };
                    foreach (Object item in requiredItems)
                    {
                        missingItemsDialogue.Text += String.Concat("- ", item.Name, " x", item.Stack);
                    }

                    Game1.activeClickableMenu = new DialogueBox(missingItemsDialogue.Text);
                }
            }
            if (Dialogue is not null)
            {
                Game1.activeClickableMenu = new DialogueBox(Dialogue.Text);
            }
            if (Message is not null)
            {
                Game1.addHUDMessage(new HUDMessage(Message.Text, (int)Message.Icon + 1));
            }
        }

        // Vanilla shop related
        private void HandleVanillaShopMenu(string shopName)
        {
            switch (shopName.ToLower())
            {
                case "clint":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithStock(), 0, "Clint");
                    return;
                case "desertmerchant":
                    Game1.activeClickableMenu = new ShopMenu(Desert.getDesertMerchantTradeStock(Game1.player), 0, "DesertTrade", onDesertTraderPurchase);
                    return;
                case "dwarf":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0, "Dwarf");
                    return;
                case "geodes":
                    Game1.activeClickableMenu = new GeodeMenu();
                    return;
                case "gus":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, "Gus", (item, farmer, amount) => onGenericPurchase(SynchronizedShopStock.SynchedShop.Saloon, item, farmer, amount));
                    return;
                case "harvey":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getHospitalStock());
                    return;
                case "itemrecovery":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(), 0, "Marlon_Recovery");
                    return;
                case "krobus":
                    Game1.activeClickableMenu = new ShopMenu((Game1.getLocationFromName("Sewer") as Sewer).getShadowShopStock(), 0, "KrobusGone", null);
                    return;
                case "marlon":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(), 0, "Marlon");
                    return;
                case "marnie":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(), 0, "Marnie");
                    return;
                case "pierre":
                    Game1.activeClickableMenu = new ShopMenu(new SeedShop().shopStock(), 0, "Pierre");
                    return;
                case "qi":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getQiShopStock(), 2);
                    return;
                case "sandy":
                    Game1.activeClickableMenu = new ShopMenu(BetterBuildings.modHelper.Reflection.GetMethod(Game1.currentLocation, "sandyShopStock").Invoke<Dictionary<ISalable, int[]>>(), 0, "Sandy", (item, farmer, amount) => onGenericPurchase(SynchronizedShopStock.SynchedShop.Sandy, item, farmer, amount));
                    return;
                case "robin":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                    return;
                case "travelingmerchant":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getTravelingMerchantStock((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)), 0, "Traveler", Utility.onTravelingMerchantShopPurchase);
                    return;
                case "toolupgrades":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithUpgradeStock(Game1.player), 0, "ClintUpgrade");
                    return;
                case "willy":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0, "Willy");
                    return;
            }
        }

        private bool onDesertTraderPurchase(ISalable item, Farmer who, int amount)
        {
            if (item.Name == "Magic Rock Candy")
            {
                Desert.boughtMagicRockCandy = true;
            }
            return false;
        }

        private bool onGenericPurchase(SynchronizedShopStock.SynchedShop synchedShop, ISalable item, Farmer who, int amount)
        {
            Game1.player.team.synchronizedShopStock.OnItemPurchased(synchedShop, item, amount);
            return false;
        }
    }
}
