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

        public string Condition { get; set; }
        public string[] ModDataFlags { get; set; }

        public DialogueAction Dialogue { get; set; }
        public MessageAction Message { get; set; }
        public WarpAction Warp { get; set; }
        public QuestionResponseAction DialogueWithChoices { get; set; }
        public ModifyInventory ModifyInventory { get; set; }
        public List<ModifyModDataAction> ModifyFlags { get; set; }
        public List<SpecialAction> ConditionalActions { get; set; }

        public void Trigger(Farmer who, GenericBuilding building)
        {
            if (ConditionalActions is not null)
            {
                var validAction = ConditionalActions.Where(d => building.ValidateConditions(d.Condition, d.ModDataFlags)).FirstOrDefault();
                if (validAction is not null)
                {
                    validAction.Trigger(who, building);
                }
            }
            if (Dialogue is not null)
            {
                Game1.activeClickableMenu = new DialogueBox(HandleSpecialTextTokens(Dialogue.Text));
            }
            if (DialogueWithChoices is not null && building.ValidateConditions(this.Condition, this.ModDataFlags))
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
            if (ModifyFlags is not null)
            {
                foreach (var modifyFlag in ModifyFlags)
                {
                    // If FlagType is Temporary, then remove it from the building's modData before saving
                    var flagKey = String.Concat(ModDataKeys.FLAG_BASE, ".", modifyFlag.Name.ToLower());
                    if (modifyFlag.Operation is OperationName.Add)
                    {
                        building.modData[flagKey] = modifyFlag.Type.ToString();
                    }
                    else if (modifyFlag.Operation is OperationName.Remove && building.modData.ContainsKey(flagKey))
                    {
                        building.modData.Remove(flagKey);
                    }
                }
            }
            if (Warp is not null)
            {
                Game1.warpFarmer(Warp.Map, Warp.DestinationTile.X, Warp.DestinationTile.Y, Warp.FacingDirection == -1 ? who.FacingDirection : Warp.FacingDirection);
            }
            if (ModifyInventory is not null)
            {
                if (ModifyInventory.Operation == OperationName.Add && Int32.TryParse(ModifyInventory.ItemId, out int id))
                {
                    Item itemToAdd = new StardewValley.Object(id, ModifyInventory.Quantity);
                    if (itemToAdd is not null && who.couldInventoryAcceptThisItem(itemToAdd))
                    {
                        who.addItemToInventoryBool(itemToAdd);
                    }
                }
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
