using Microsoft.Xna.Framework;
using SolidFoundations.Framework.Interfaces.Internal;
using SolidFoundations.Framework.UI;
using SolidFoundations.Framework.Utilities;
using SolidFoundations.Framework.Utilities.Backport;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class SpecialAction
    {
        public enum MessageType
        {
            Achievement,
            Quest,
            Error,
            Stamina,
            Health
        }

        public enum FlagType
        {
            Temporary, // Removed on a new day start
            Permanent
        }

        public enum OperationName
        {
            Add,
            Remove
        }

        public enum StoreType
        {
            Vanilla,
            STF
        }

        public string Condition { get; set; }
        public string[] ModDataFlags { get; set; }

        public DialogueAction Dialogue { get; set; }
        public MessageAction Message { get; set; }
        public WarpAction Warp { get; set; }
        public QuestionResponseAction DialogueWithChoices { get; set; }
        public ModifyInventory ModifyInventory { get; set; }
        public OpenShopAction OpenShop { get; set; }
        public List<ModifyModDataAction> ModifyFlags { get; set; }
        public List<SpecialAction> ConditionalActions { get; set; }
        public BroadcastAction Broadcast { get; set; }

        public void Trigger(Farmer who, GenericBuilding building, Point tile)
        {
            if (ConditionalActions is not null)
            {
                var validAction = ConditionalActions.Where(d => building.ValidateConditions(d.Condition, d.ModDataFlags)).FirstOrDefault();
                if (validAction is not null)
                {
                    validAction.Trigger(who, building, tile);
                }
            }
            if (Dialogue is not null)
            {
                Game1.activeClickableMenu = new DialogueBox(HandleSpecialTextTokens(Dialogue.Text));
            }
            if (DialogueWithChoices is not null && building.ValidateConditions(this.Condition, this.ModDataFlags))
            {
                List<Response> responses = new List<Response>();
                foreach (var response in DialogueWithChoices.Responses.Where(r => String.IsNullOrEmpty(r.Text) is false).OrderBy(r => DialogueWithChoices.ShuffleResponseOrder ? Game1.random.Next() : DialogueWithChoices.Responses.IndexOf(r)))
                {
                    responses.Add(new Response(DialogueWithChoices.Responses.IndexOf(response).ToString(), HandleSpecialTextTokens(response.Text)));
                }

                // Unable to use the vanilla method, as the afterQuestion gets cleared before the second instance of DialogueBox can call it (due to first instance closing)
                //who.currentLocation.createQuestionDialogue(HandleSpecialTextTokens(DialogueWithChoices.Question), responses.ToArray(), new GameLocation.afterQuestionBehavior((who, whichAnswer) => DialogueResponsePicked(who, building, tile, whichAnswer)));

                var dialogue = new SpecialActionDialogueBox(who.currentLocation, HandleSpecialTextTokens(DialogueWithChoices.Question), responses, (who, whichAnswer) => DialogueResponsePicked(who, building, tile, whichAnswer));
                Game1.activeClickableMenu = dialogue;
                dialogue.SetUp();
            }
            if (Message is not null)
            {
                Game1.addHUDMessage(new HUDMessage(Message.Text, (int)Message.Icon + 1));
            }
            if (ModifyFlags is not null)
            {
                SpecialAction.HandleModifyingBuildingFlags(building, ModifyFlags);
            }
            if (Warp is not null)
            {
                Game1.warpFarmer(Warp.Map, Warp.DestinationTile.X, Warp.DestinationTile.Y, Warp.FacingDirection == -1 ? who.FacingDirection : Warp.FacingDirection);
            }
            if (ModifyInventory is not null)
            {
                var quantity = ModifyInventory.Quantity;
                if (quantity <= 0)
                {
                    if (ModifyInventory.MaxCount < ModifyInventory.MinCount)
                    {
                        ModifyInventory.MaxCount = ModifyInventory.MinCount;
                    }

                    quantity = new Random((int)((long)Game1.uniqueIDForThisGame + who.DailyLuck + Game1.stats.DaysPlayed * 500)).Next(ModifyInventory.MinCount, ModifyInventory.MaxCount + 1);
                }

                if (quantity > 0 && Toolkit.CreateItemByID(ModifyInventory.ItemId, quantity, ModifyInventory.Quality) is Item item && item is not null)
                {
                    if (ModifyInventory.Operation == OperationName.Add && item is not null)
                    {
                        if (who.couldInventoryAcceptThisItem(item))
                        {
                            who.addItemToInventoryBool(item);
                        }
                    }
                    else if (ModifyInventory.Operation == OperationName.Remove && item is not null)
                    {
                        InventoryManagement.ConsumeItemBasedOnQuantityAndQuality(who, item, quantity, ModifyInventory.Quality);
                    }
                }
            }
            if (OpenShop is not null)
            {
                if (OpenShop.Type is StoreType.Vanilla)
                {
                    HandleVanillaShopMenu(OpenShop.Name, who);
                }
                else if (OpenShop.Type is StoreType.STF && SolidFoundations.apiManager.GetShopTileFrameworkApi() is not null)
                {
                    SolidFoundations.apiManager.GetShopTileFrameworkApi().OpenItemShop(OpenShop.Name);
                }
            }
            if (Broadcast is not null)
            {
                var triggeredArgs = new IApi.BroadcastEventArgs()
                {
                    BuildingId = building.Id,
                    Building = building,
                    Farmer = who,
                    TriggerTile = tile,
                    Message = Broadcast.Message
                };
                SolidFoundations.api.OnSpecialActionTriggered(triggeredArgs);
            }
        }

        private void DialogueResponsePicked(Farmer who, GenericBuilding building, Point tile, string answerTextIndex)
        {
            int answerIndex = -1;

            if (!int.TryParse(answerTextIndex, out answerIndex) || DialogueWithChoices is null)
            {
                return;
            }

            ResponseAction response = DialogueWithChoices.Responses[answerIndex];
            if (response.SpecialAction is null)
            {
                return;
            }

            response.SpecialAction.Trigger(who, building, tile);
        }

        private string HandleSpecialTextTokens(string text)
        {
            var dialogueText = TextParser.ParseText(text);
            dialogueText = SolidFoundations.modHelper.Reflection.GetMethod(new Dialogue(dialogueText, null), "checkForSpecialCharacters").Invoke<string>(dialogueText);

            return dialogueText;
        }

        // Vanilla shop related
        private void HandleVanillaShopMenu(string shopName, Farmer who)
        {
            switch (shopName.ToLower())
            {
                case "clintshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithStock(), 0, "Clint");
                    return;
                case "deserttrader":
                    Game1.activeClickableMenu = new ShopMenu(Desert.getDesertMerchantTradeStock(who), 0, "DesertTrade", onDesertTraderPurchase);
                    return;
                case "dwarfshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0, "Dwarf");
                    return;
                case "geodes":
                    Game1.activeClickableMenu = new GeodeMenu();
                    return;
                case "gusshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, "Gus", (item, farmer, amount) => onGenericPurchase(SynchronizedShopStock.SynchedShop.Saloon, item, farmer, amount));
                    return;
                case "harveyshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getHospitalStock());
                    return;
                case "itemrecovery":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(), 0, "Marlon_Recovery");
                    return;
                case "krobusshop":
                    Game1.activeClickableMenu = new ShopMenu((Game1.getLocationFromName("Sewer") as Sewer).getShadowShopStock(), 0, "KrobusGone", null);
                    return;
                case "marlonshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(), 0, "Marlon");
                    return;
                case "marnieshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(), 0, "Marnie");
                    return;
                case "pierreshop":
                    Game1.activeClickableMenu = new ShopMenu(new SeedShop().shopStock(), 0, "Pierre");
                    return;
                case "qishop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getQiShopStock(), 2);
                    return;
                case "sandyshop":
                    Game1.activeClickableMenu = new ShopMenu(SolidFoundations.modHelper.Reflection.GetMethod(Game1.currentLocation, "sandyShopStock").Invoke<Dictionary<ISalable, int[]>>(), 0, "Sandy", (item, farmer, amount) => onGenericPurchase(SynchronizedShopStock.SynchedShop.Sandy, item, farmer, amount));
                    return;
                case "robinshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                    return;
                case "travelingmerchant":
                case "travellingmerchant":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getTravelingMerchantStock((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)), 0, "Traveler", Utility.onTravelingMerchantShopPurchase);
                    return;
                case "toolupgrades":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithUpgradeStock(who), 0, "ClintUpgrade");
                    return;
                case "willyshop":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(who), 0, "Willy");
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
            who.team.synchronizedShopStock.OnItemPurchased(synchedShop, item, amount);
            return false;
        }

        internal static void HandleModifyingBuildingFlags(GenericBuilding building, List<ModifyModDataAction> modifyFlags)
        {
            foreach (var modifyFlag in modifyFlags)
            {
                // If FlagType is Temporary, then remove it from the building's modData before saving
                var flagKey = String.Concat(ModDataKeys.FLAG_BASE, ".", modifyFlag.Name.ToLower());
                if (modifyFlag.Operation is OperationName.Add)
                {
                    building.modData[flagKey] = modifyFlag.Type.ToString();
                }
                else if (modifyFlag.Operation is OperationName.Remove && building.modData.ContainsKey(flagKey))
                {
                    building.modData.Remove(flagKey);
                }
            }
        }
    }
}
