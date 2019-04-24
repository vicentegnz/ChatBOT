using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;


namespace ChatBOT.Dialogs
{
    public class ScheduleDialog : BaseDialog
    {
        public ScheduleDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
        }

        public new static string Id => "ScheduleDialog";
    }
}
