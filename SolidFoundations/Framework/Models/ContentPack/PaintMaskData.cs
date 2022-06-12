using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class PaintMaskData
    {
        public string Name { get; set; }
        public int MinBrightness { get; set; } = -100;
        public int MaxBrightness { get; set; } = 100;
    }
}
