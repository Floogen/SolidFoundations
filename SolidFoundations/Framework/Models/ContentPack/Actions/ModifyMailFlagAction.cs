using static SolidFoundations.Framework.Models.ContentPack.Actions.SpecialAction;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class ModifyMailFlagAction
    {
        public string Name { get; set; }
        public OperationName Operation { get; set; } = OperationName.Add;
    }
}
