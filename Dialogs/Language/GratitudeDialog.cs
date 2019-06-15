using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class GratitudeDialog : BaseDialog
    {
        #region Properties

        private readonly TemplateEngine _lgEngine;
      
        #endregion

        public GratitudeDialog(string dialogId) : base(dialogId)
        {
            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "GratitudeDialog.lg" });
            _lgEngine = new TemplateEngine().AddFile(fullPath);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{ SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("Gratitude", null));

            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }
    }
}
