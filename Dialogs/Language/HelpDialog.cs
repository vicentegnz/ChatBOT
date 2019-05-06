using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace ChatBOT.Dialogs
{
    public sealed class HelpDialog : BaseDialog
    {

        #region "Properties"
        private readonly ILanguageService _helpService;
        #endregion

        public HelpDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            AddStep(async (stepContext, cancellationToken) =>
            {
                await stepContext.Context.SendActivityAsync("Las cosas que puedo hacer son, consultar la ficha de una asignatura , información de un profesor, como puede ser su horario de tutoria, el horario del grado, y cualquier otra consulta relacionada con la UNEX.");
                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
            });
        }

        public new static string Id => "HelpDialog";
    }
}