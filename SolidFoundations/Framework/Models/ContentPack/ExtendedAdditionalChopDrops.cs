using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedAdditionalChopDrops : AdditionalChopDrops
    {
        [ContentSerializer(Optional = true)]
        public int Quality;

        [ContentSerializer(Optional = true)]
        public string PreserveType;

        [ContentSerializer(Optional = true)]
        public string PreserveID;

        [ContentSerializer(Optional = true)]
        public int AddPrice;

        [ContentSerializer(Optional = true)]
        public float MultiplyPrice;
    }
}
