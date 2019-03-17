using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class GoodByeDialog : WaterfallDialog
    {
        public GoodByeDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
        }

        public static string Id => "goodByeDialog";
    }
}
