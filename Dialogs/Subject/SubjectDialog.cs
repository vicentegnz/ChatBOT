

using System.Collections.Generic;
using ChatBOT.Core;
using Microsoft.Bot.Builder.Dialogs;

namespace ChatBOT.Dialogs
{
    public sealed class SubjectDialog : WaterfallDialog
    {
        public SubjectDialog(string dialogId, IOpenDataService openDataService,IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {

            AddStep(async (stepContext, cancellationToken) =>
            {
                var centers = openDataService.GetStudyCenters();

                await stepContext.Context.SendActivityAsync($"He encontrado todos estos centros: {centers.Count}");

                return await stepContext.BeginDialogAsync(MainLuisDialog.Id, cancellationToken);
            });

        }

        public new static string Id => "Subjectdialog";
    }
}
