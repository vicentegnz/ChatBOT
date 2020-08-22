using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Domain.Dialogs.Language
{
    public class GoodByeDialog : BaseDialog 
    {
        #region Properties

        private readonly Templates _lgEngine;

        #endregion

        public GoodByeDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "GoodByeDialog.lg" });
            _lgEngine = Templates.ParseFile(fullPath);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("GoodBye").ToString());

            return await stepContext.EndDialogAsync();
        }
        
    }
}
