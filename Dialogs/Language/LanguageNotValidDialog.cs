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
    public sealed class LanguageNotValidDialog : BaseDialog
    {

        #region "Properties"
        #endregion

        public LanguageNotValidDialog(string dialogId,  IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            AddStep(async (stepContext, cancellationToken) =>
            {
                await stepContext.Context.SendActivityAsync($"El lenguage que estás utilizando no es el adecuado.");
                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
            });
        }

        public new static string Id => "languageNotValidDialog";
    }
}