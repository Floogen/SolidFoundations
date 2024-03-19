using System.Collections.Generic;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class InputFilter
    {
        public List<RestrictedItem> RestrictedItems;

        public string FilteredItemMessage;

        public string InputChest;

        public class RestrictedItem
        {
            public List<string> RequiredTags { get; set; }
            public int MaxAllowed { get; set; } = -1;
            public bool RejectWhileProcessing { get; set; }
            public string Condition { get; set; }
            public string[] ModDataFlags { get; set; }
        }
    }
}
