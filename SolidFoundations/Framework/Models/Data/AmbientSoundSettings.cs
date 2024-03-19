using Microsoft.Xna.Framework;

namespace SolidFoundations.Framework.Models.Data
{
    public class AmbientSoundSettings
    {
        public Point Source { get; set; }
        public float MaxDistance { get; set; } = 1024f;
    }
}
