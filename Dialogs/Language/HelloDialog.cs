using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    public class HelloDialog : BaseDialog
    {
        public HelloDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            AddStep(async (stepContext, cancellationToken) =>
            {
                await stepContext.Context.SendActivityAsync("Hola, soy Nexo 🤖 un asistente virtual de la Unex. Estoy deseando escucharte.");

                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
            });
        }

        public new static string Id => "HelloDialog";

    }
}
