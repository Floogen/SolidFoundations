using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SolidFoundations.Framework.Models.ContentPack.Actions.SpecialAction;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class ModifyModDataAction
    {
        public string Name { get; set; }
        public FlagType Type { get; set; } = FlagType.Temporary;
        public OperationName Operation { get; set; } = OperationName.Add;
    }
}
