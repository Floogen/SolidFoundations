using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class TextureAnimation
    {
        public int Frame { get; set; }
        public int RowOffset { get; set; }
        public int Duration { get; set; } = 1000;
    }
}
