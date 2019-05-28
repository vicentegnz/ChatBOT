using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class GoodByeDialog : BaseDialog 
    {
        #region Properties

        private readonly TemplateEngine _lgEngine;

        #endregion

        public GoodByeDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            string fullPath = Path.Combine(new string[] { ".", "Dialogs", "Language", "GoodByeDialog.lg" });
            _lgEngine = TemplateEngine.FromFiles(fullPath);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("GoodBye", null));

            return await stepContext.EndDialogAsync();
        }
        
    }
}
