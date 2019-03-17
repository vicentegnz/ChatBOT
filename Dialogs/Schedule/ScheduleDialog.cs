using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;


namespace ChatBOT.Dialogs
{
    public class ScheduleDialog : WaterfallDialog
    {
        public ScheduleDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
        }

        public static string Id => "ScheduleDialog";
    }
}
