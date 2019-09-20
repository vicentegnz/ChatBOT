using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class HelpDialog : BaseDialog
    {
        #region Properties
        private readonly TemplateEngine _lgEngine;
        #endregion

        public HelpDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "HelpDialog.lg" });
            _lgEngine = new TemplateEngine().AddFile(fullPath);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("Help", null));

            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }

    }
}