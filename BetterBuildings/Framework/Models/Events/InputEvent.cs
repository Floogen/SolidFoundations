using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class InputEvent
    {
        public List<ItemModel> RequiredItems { get; set; } = new List<ItemModel>();
        public MessageEvent BadInputMessage { get; set; } = new MessageEvent("Nothing interesting happens", MessageType.Quest);
        public bool StartProduction { get; set; } = true;
    }
}
