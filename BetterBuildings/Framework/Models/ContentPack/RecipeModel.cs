using BetterBuildings.Framework.Models.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.ContentPack
{
    public class RecipeModel
    {
        public int ProcessingTimeInGameMinutes { get; set; }
        public bool FinishAtDayStart { get; set; }
        public List<ItemModel> InputItems { get; set; }
        public List<ItemModel> OutputItems { get; set; }
    }
}
