using BetterBuildings.Framework.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.Events
{
    public enum MessageType
    {
        Achievement,
        Quest,
        Error,
        Stamina,
        Health
    }

    public class MessageEvent
    {
        public string Text { get; set; } = "Nothing interesting happens";
        public MessageType Icon { get; set; } = MessageType.Quest;

        public MessageEvent()
        {

        }

        public MessageEvent(string message, MessageType icon)
        {
            Text = message;
            Icon = icon;
        }
    }
}
