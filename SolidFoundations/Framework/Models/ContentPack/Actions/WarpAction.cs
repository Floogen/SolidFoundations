using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class WarpAction
    {
        public string Map { get; set; }
        public Point DestinationTile { get; set; }
        public int FacingDirection { get; set; } = 1;
    }
}
