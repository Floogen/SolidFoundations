using static SolidFoundations.Framework.Models.ContentPack.Actions.SpecialAction;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class OpenShopAction
    {
        public string Name { get; set; }
        public StoreType Type { get; set; } = StoreType.Vanilla;
    }
}
