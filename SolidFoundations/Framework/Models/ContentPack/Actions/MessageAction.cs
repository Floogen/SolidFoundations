using static SolidFoundations.Framework.Models.ContentPack.Actions.SpecialAction;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class MessageAction
    {
        public string Text { get; set; } = "Nothing interesting happens";
        public MessageType Icon { get; set; } = MessageType.Quest;

        public MessageAction()
        {

        }

        public MessageAction(string message, MessageType icon)
        {
            Text = message;
            Icon = icon;
        }
    }
}
