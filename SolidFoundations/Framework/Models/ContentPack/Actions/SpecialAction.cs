using SolidFoundations.Framework.Utilities;
using SolidFoundations.Framework.Utilities.Backport;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack.Actions
{
    public class SpecialAction
    {
        public enum MessageType
        {
            Achievement,
            Quest,
            Error,
            Stamina,
            Health
        }

        public enum FlagType
        {
            Temporary, // Removed on a new day start
            Permanent
        }

        public enum OperationName
        {
            Add,
            Remove
        }

        public DialogueAction Dialogue { get; set; }
        public MessageAction Message { get; set; }
        public WarpAction Warp { get; set; }
        public QuestionResponseAction DialogueWithChoices { get; set; }
        public ModifyModDataAction ModifyFlag { get; set; }

        public void Trigger(Farmer who, GenericBuilding building)
        {
            if (Dialogue is not null)
            {
                Game1.activeClickableMenu = new DialogueBox(HandleSpecialTextTokens(Dialogue.Text));
            }
            if (DialogueWithChoices is not null)
            {
                List<Response> responses = new List<Response>();
                foreach (var response in DialogueWithChoices.Responses.Where(r => r.SpecialAction is not null))
                {
                    responses.Add(new Response(responses.Count.ToString(), HandleSpecialTextTokens(response.Text)));
                }

                who.currentLocation.createQuestionDialogue(HandleSpecialTextTokens(DialogueWithChoices.Question), responses.ToArray(), new GameLocation.afterQuestionBehavior((who, whichAnswer) => DialogueResponsePicked(who, building, whichAnswer)));
            }
            if (Message is not null)
            {
                Game1.addHUDMessage(new HUDMessage(Message.Text, (int)Message.Icon + 1));
            }
            if (ModifyFlag is not null)
            {
                // If FlagType is Temporary, then remove it from the building's modData before saving
                var flagKey = String.Concat(ModDataKeys.FLAG_BASE, ".", ModifyFlag.Name.ToLower());
                if (ModifyFlag.Operation is OperationName.Add)
                {
                    building.modData[flagKey] = ModifyFlag.Type.ToString();
                }
                else if (ModifyFlag.Operation is OperationName.Remove && building.modData.ContainsKey(flagKey))
                {
                    building.modData.Remove(flagKey);
                }
            }
            if (Warp is not null)
            {
                Game1.warpFarmer(Warp.Map, Warp.DestinationTile.X, Warp.DestinationTile.Y, Warp.FacingDirection == -1 ? who.FacingDirection : Warp.FacingDirection);
            }
        }

        private void DialogueResponsePicked(Farmer who, GenericBuilding building, string answerTextIndex)
        {
            int answerIndex = -1;

            if (!int.TryParse(answerTextIndex, out answerIndex) || DialogueWithChoices is null)
            {
                return;
            }

            ResponseAction response = DialogueWithChoices.Responses[answerIndex];
            if (response.SpecialAction is null)
            {
                return;
            }

            response.SpecialAction.Trigger(who, building);
        }

        private string HandleSpecialTextTokens(string text)
        {
            var dialogueText = TextParser.ParseText(text);
            dialogueText = SolidFoundations.modHelper.Reflection.GetMethod(new Dialogue(dialogueText, null), "checkForSpecialCharacters").Invoke<string>(dialogueText);

            return dialogueText;
        }
    }
}
