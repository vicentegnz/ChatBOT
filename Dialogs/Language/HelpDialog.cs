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

        public HelpDialog(string dialogId, ILanguageService helpService, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            _helpService = helpService;

            AddStep(async (stepContext, cancellationToken) =>
            {
                await stepContext.Context.SendActivityAsync($"{_helpService.GetText()}");
                return await stepContext.EndDialogAsync();
            });
        }

        public new static string Id => "HelpDialog";
    }
}