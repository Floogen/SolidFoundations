using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.Data
{
    public class RandomRange
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public int Get(Random random)
        {
            return random.Next(Min, Max);
        }
    }
}
