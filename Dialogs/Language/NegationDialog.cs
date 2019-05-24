using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    public class NegationDialog : BaseDialog
    {

        public NegationDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        { 
            AddStep(async (stepContext, cancellationToken) =>
                {
                await stepContext.Context.SendActivityAsync($"Lo siento por no haberte ayudado, de todas formas sigo por aquí.");
                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, null, cancellationToken);

            });
     }

    public new static string Id => "NegationDialog";

    }
}
