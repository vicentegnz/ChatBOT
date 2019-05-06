

using System.Collections.Generic;
using ChatBOT.Core;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    public sealed class SubjectDialog : WaterfallDialog
    {
        public SubjectDialog(string dialogId, ISubjectService subjectService,IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            AddStep(async (stepContext, cancellationToken) =>
            {
                var centers = subjectService.GetStudyCenters();
                var subjects = subjectService.GetDegrees();

                return await stepContext.PromptAsync("textPrompt",
                    new PromptOptions
                    {
                        Prompt = stepContext.Context.Activity.CreateReply("¿Podrías indicarme el nombre del profesor?")
                    });
            });

        }

        public new static string Id => "Subjectdialog";
    }
}
