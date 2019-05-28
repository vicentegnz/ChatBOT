﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace ChatBOT.Dialogs
{
    public class NegationDialog : BaseDialog
    {
        #region Properties
        private readonly TemplateEngine _lgEngine;
        #endregion

        public NegationDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            string fullPath = Path.Combine(new string[] { ".", "Dialogs", "Language", "NegationDialog.lg" });
            _lgEngine = TemplateEngine.FromFiles(fullPath);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("Negation",null));
            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }
    
    }
}
