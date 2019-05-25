using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public sealed class LanguageNotValidDialog : BaseDialog
    {

        #region "Properties"
        #endregion

        public LanguageNotValidDialog(string dialogId,  IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"El lenguage que estás utilizando no es el adecuado.");
            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }

    }
}