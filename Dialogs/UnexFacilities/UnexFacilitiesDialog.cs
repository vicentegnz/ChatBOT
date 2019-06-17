using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public class UnexFacilitiesDialog : BaseDialog
    {
        #region Properties
        private readonly IUnexFacilitiesService _unexFacilitiesService;
        private readonly TemplateEngine _lgEngine;

        #endregion
        public UnexFacilitiesDialog(string dialogId, IUnexFacilitiesService unexFacilititesService, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            _lgEngine = new TemplateEngine().AddFile(Path.Combine(new string[] { ".", ".", "Resources", "UnexFacilitiesDialog.lg" }));
            _unexFacilitiesService = unexFacilititesService;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
              {
                                        IntroFacilitiesQuestionStepAsync,
                                        IntermediateFacilitiesAnswerStepAsync,                                   
                                        //QuestionSubjectStepAsync,
                                        //EndQuestionStepAsync,

              }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> IntroFacilitiesQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<UnexFacilitieModel> facilities = _unexFacilitiesService.GetUnexFacilities();

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("IntroFacilities", null));

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskFacilitieAgain", null)),
                    Choices = ChoiceFactory.ToChoices(facilities.Select(x => x.Name).ToList()),
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskFacilitie", null))
                });
        }

        private async Task<DialogTurnResult> IntermediateFacilitiesAnswerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;
            var state = await GetNexoBotState(stepContext);

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("AnswerIntermediateFacilities", state.SubjectModel));

            return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }


    }
}
