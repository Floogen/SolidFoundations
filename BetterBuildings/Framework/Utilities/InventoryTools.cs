using BetterBuildings.Framework.Models.ContentPack;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace BetterBuildings.Framework.Utilities
{
    public class InventoryTools
    {
        public static List<Item> GetActualRequiredItems(List<ItemModel> requiredItems)
        {
            List<Item> items = new List<Item>();
            foreach (var genericItem in requiredItems)
            {
                KeyValuePair<int, string>? gameObjectData = Game1.objectInformation.FirstOrDefault(pair => pair.Value.Split('/')[0].Equals(genericItem.Name, StringComparison.OrdinalIgnoreCase));
                if (gameObjectData is not null)
                {
                    items.Add(new Object((int)(gameObjectData?.Key), genericItem.Quantity) { Quality = genericItem.GetActualQuality() });
                }
            }

            return items;
        }

        public static bool IsRequiredItem(Item targetItem, List<Item> requiredItems)
        {
            if (targetItem is null)
            {
                return false;
            }

            foreach (Object requiredItem in requiredItems)
            {
                if (requiredItem.Name.Equals(targetItem.Name) && targetItem.Stack >= requiredItem.Stack)
                {
                    if (requiredItem.Quality == -1 || (targetItem is Object itemObject && itemObject is not null && itemObject.Quality >= (int)requiredItem.Quality))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsHoldingRequiredItem(List<Item> requiredItems)
        {
            if (Game1.player.ActiveObject is null)
            {
                return false;
            }

            var activeItem = Game1.player.ActiveObject;
            foreach (Object requiredItem in requiredItems)
            {
                if (requiredItem.Name.Equals(activeItem.Name) && activeItem.Stack >= requiredItem.Stack)
                {
                    if (requiredItem.Quality == -1 || (activeItem is Object itemObject && itemObject is not null && itemObject.Quality >= (int)requiredItem.Quality))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasRoomForItems(List<Item> requiredItems)
        {
            int freeSpace = Game1.player.freeSpotsInInventory();
            foreach (var item in requiredItems)
            {
                freeSpace -= 1;

                if (freeSpace < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasRequiredItems(List<Item> sourceItems, List<Item> requiredItems)
        {
            bool hasEverythingRequired = true;
            foreach (Object item in requiredItems)
            {
                if (!HasItemWithRequiredQuantityAndQuality(sourceItems, item, item.Stack, item.Quality))
                {
                    hasEverythingRequired = false;
                }
            }

            return hasEverythingRequired;
        }

        public static bool HasRequiredItemsInInventory(List<Item> requiredItems)
        {
            bool hasEverythingRequired = true;
            foreach (Object item in requiredItems)
            {
                if (!HasInventoryItemWithRequiredQuantityAndQuality(item, item.Stack, item.Quality))
                {
                    hasEverythingRequired = false;
                }
            }

            return hasEverythingRequired;
        }

        public static bool HasInventoryItemWithRequiredQuantityAndQuality(Item targetItem, int quantity, int quality = -1)
        {
            return HasItemWithRequiredQuantityAndQuality(Game1.player.Items.ToList(), targetItem, quantity, quality);
        }

        public static bool HasItemWithRequiredQuantityAndQuality(List<Item> inputItems, Item targetItem, int quantity, int quality = -1)
        {
            int requiredCount = quantity;
            foreach (var item in inputItems.Where(i => i is not null && i.Name.Equals(targetItem.Name)))
            {
                if (quality == -1 || (item is Object itemObject && itemObject is not null && itemObject.Quality >= (int)quality))
                {
                    requiredCount -= item.Stack;
                }
            }

            return requiredCount <= 0;
        }

        public static void ConsumeItemBasedOnQuantityAndQuality(Chest chest, Item targetItem, int quantity, int quality = -1)
        {
            int requiredCount = quantity;
            foreach (var item in chest.items.Where(i => i is not null && i.Name.Equals(targetItem.Name)).ToList())
            {
                if (requiredCount <= 0)
                {
                    break;
                }

                if (quality == -1 || (item is Object itemObject && itemObject is not null && itemObject.Quality >= (int)quality))
                {
                    if (item.Stack <= requiredCount)
                    {
                        requiredCount -= item.Stack;
                        chest.items.Remove(item);
                    }
                    else
                    {
                        item.Stack -= requiredCount;
                        break;
                    }
                }
            }
        }

        public static void ConsumeItemBasedOnQuantityAndQuality(Farmer who, Item targetItem, int quantity, int quality = -1)
        {
            int requiredCount = quantity;
            foreach (var item in who.Items.Where(i => i is not null && i.Name.Equals(targetItem.Name)).ToList())
            {
                if (requiredCount <= 0)
                {
                    break;
                }

                if (quality == -1 || (item is Object itemObject && itemObject is not null && itemObject.Quality >= (int)quality))
                {
                    if (item.Stack <= requiredCount)
                    {
                        requiredCount -= item.Stack;
                        who.Items.Remove(item);
                    }
                    else
                    {
                        item.Stack -= requiredCount;
                        break;
                    }
                }
            }
        }
    }
}
