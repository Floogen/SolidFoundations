using Microsoft.Xna.Framework;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class WarpAction
    {
        public string Map { get; set; }
        public Point DestinationTile { get; set; }
        public int FacingDirection { get; set; } = 1;
        public bool IsMagic { get; set; }
    }
}
