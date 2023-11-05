using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedGenericSpawnItemDataWithCondition : GenericSpawnItemDataWithCondition
    {
        // TODO: Review these properties, convert to ModData     


        [Obsolete("Kept for backwards compatibility. Use PreserveId instead.")]
        public string PreserveID { set { PreserveId = value; } }
        public string PreserveId { get; set; }
        public string PreserveType { get; set; }


        [Obsolete("Kept for backwards compatibility. Use PriceModifiers instead.")]
        public int AddPrice { get; set; }
        [Obsolete("Kept for backwards compatibility. Use PriceModifiers instead.")]
        public float MultiplyPrice { get; set; } = 1f;

        public bool CopyPrice { get; set; }
        public List<QuantityModifier> PriceModifiers { get; set; }
        public QuantityModifier.QuantityModifierMode PriceModifierMode { get; set; }

        
        public string[] ModDataFlags { get; set; }
    }
}
