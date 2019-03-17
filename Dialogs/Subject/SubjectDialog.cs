

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    public sealed class SubjectDialog : WaterfallDialog
    {
        public SubjectDialog(string dialogId, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
        }

        public static string Id => "Subjectdialog";
    }
}
