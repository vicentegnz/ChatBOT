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
                await stepContext.Context.SendActivityAsync(@"Las cosas que puedo hacer son:
                    - Consultar la ficha de una asignatura.
                    - Consultar información de un profesor, como puede ser su horario de tutoria.
                    - Consultar el horario del grado.
                    - Otras consultas relacionadas con la UNEX. (Basadas en el FAQ)");

                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
            });
        }

        public new static string Id => "HelpDialog";
    }
}