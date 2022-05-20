using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class Sequence
    {
        public int Frame { get; set; }
        public int Duration { get; set; }
        public string Condition { get; set; } // TODO: Implement this with SDV v1.6, using the game's query system
    }
}
