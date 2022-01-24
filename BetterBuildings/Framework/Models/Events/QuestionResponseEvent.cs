using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public class QuestionResponseEvent
    {
        public string Question { get; set; }
        public List<ResponseEvent> Responses { get; set; } = new List<ResponseEvent>();
    }
}
