using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Utilities.Backport
{
    // TODO: When updated to SDV v1.6, this class should be deleted in favor of using StardewValley.Utility 
    internal static class Toolkit
    {
        public static int GetNumberOfItemThatCanBeAddedToThisInventoryList(Item item, IList<Item> list, int list_max_items)
        {
            int num = 0;
            foreach (Item item2 in list)
            {
                if (item2 == null)
                {
                    num += item.maximumStackSize();
                }
                else if (item2 != null && item2.canStackWith(item) && item2.getRemainingStackSpace() > 0)
                {
                    num += item2.getRemainingStackSpace();
                }
            }
            for (int i = 0; i < list_max_items - list.Count; i++)
            {
                num += item.maximumStackSize();
            }
            return num;
        }

        public static Item ConsumeStack(Item item, int amount)
        {
            if (amount == 0)
            {
                return item;
            }
            if (item.Stack - amount <= 0)
            {
                return null;
            }

            item.Stack -= amount;
            return item;
        }

    }
}
