using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SolidFoundations.Framework.Models.ContentPack.Actions.SpecialAction;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class ModifyInventory
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public int Quality { get; set; }
        public OperationName Operation { get; set; } = OperationName.Add;
    }
}
