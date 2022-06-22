using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.Data
{
    public class AmbientSoundSettings
    {
        public Point Source { get; set; }
        public float MaxDistance { get; set; } = 1024f;
    }
}
