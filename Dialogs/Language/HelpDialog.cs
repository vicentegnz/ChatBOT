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
    public class HelpDialog : BaseDialog
    {
        public HelpDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Las cosas que puedo hacer son, consultar la ficha de una asignatura , información de un profesor, como puede ser su horario de tutoria, el horario del grado, y cualquier otra consulta relacionada con la UNEX.");

            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }

    }
}