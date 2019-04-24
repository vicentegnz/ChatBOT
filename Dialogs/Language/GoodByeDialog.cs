using ChatBOT.Core;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class GoodByeDialog : BaseDialog 
    {
        

        public GoodByeDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            AddStep(async (stepContext, cancellationToken) =>
            {
                await stepContext.Context.SendActivityAsync(@"Hasta luego, espero haberte ayudado.");

                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
            });
        }

        public static string Id => "goodByeDialog";


    }
}
