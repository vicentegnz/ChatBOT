using ChatBOT.Core;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class GoodByeDialog : BaseDialog 
    {
        

        public GoodByeDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { SendMessageStepAsync }));
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> SendMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(@"Hasta luego, espero haberte ayudado.");

            return await stepContext.EndDialogAsync();
        }
        
    }
}
