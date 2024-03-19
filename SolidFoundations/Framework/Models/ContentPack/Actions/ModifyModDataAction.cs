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
