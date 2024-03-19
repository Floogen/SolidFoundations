using System.Collections.Generic;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class DialogueAction
    {
        public List<string> Text { get; set; }
        public SpecialAction ActionAfterDialogue { get; set; }
    }
}
