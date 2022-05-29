using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class QuestionResponseAction
    {
        public string Question { get; set; }
        public List<ResponseAction> Responses { get; set; } = new List<ResponseAction>();
        public bool ShuffleResponseOrder { get; set; }
    }
}
