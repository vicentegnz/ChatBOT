using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Domain.Dialogs.Language
{
    public class GratitudeDialog : BaseDialog
    {
        #region Properties

        private readonly Templates _lgEngine;
      
        #endregion

        public GratitudeDialog(string dialogId) : base(dialogId)
        {
            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "GratitudeDialog.lg" });
            _lgEngine =  Templates.ParseFile(fullPath);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{ SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("Gratitude").ToString());

            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }
    }
}
