using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class DialogueAction
    {
        public List<string> Text { get; set; }
        public SpecialAction ActionAfterDialogue { get; set; }
    }
}
