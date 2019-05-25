using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class GratitudeDialog : BaseDialog
    {
        public GratitudeDialog(string dialogId) : base(dialogId)
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{ SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No tienes porque agradecerlo, para eso estamos.");

            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }
    }
}
