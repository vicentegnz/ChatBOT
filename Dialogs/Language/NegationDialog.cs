using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    public class NegationDialog : BaseDialog
    {

        public NegationDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        { 
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync($"Lo siento por no haberte ayudado, de todas formas sigo por aquí.");
            return await stepContext.BeginDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }
    
    }
}
