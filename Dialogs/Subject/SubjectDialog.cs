

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBOT.Bot;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace ChatBOT.Dialogs
{
    public sealed class SubjectDialog : BaseDialog
    {
        #region Properties

        private readonly IOpenDataService _openDataService;
        private readonly TemplateEngine _lgEngine;

        #endregion

        public SubjectDialog(string dialogId, IOpenDataService openDataService, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            
            _openDataService = openDataService;

            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "SubjectDialog.lg" });
            _lgEngine = new TemplateEngine().AddFile(fullPath);

            ChoicePrompt choicePrompt = new ChoicePrompt(nameof(ChoicePrompt));
            choicePrompt.ChoiceOptions = new ChoiceFactoryOptions { IncludeNumbers = false };
            //choicePrompt.RecognizerOptions = new FindChoicesOptions { AllowPartialMatches = true };
            AddDialog(choicePrompt);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
              {
                                        QuestionCenterStepAsync,
                                        QuestionDegreeStepAsync,
                                        QuestionSubjectStepAsync,
                                        EndQuestionStepAsync,

              }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> QuestionCenterStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<StudyCenterModel> centers = _openDataService.GetStudyCenters();

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("InitSubjectDialog", null));

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskCenterName", null)),
                Choices = ChoiceFactory.ToChoices(centers.Select(x => x.Name).ToList()),
                RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskCenterNameAgain", null))
            });
        }

        private async Task<DialogTurnResult> QuestionDegreeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;
            var state = await GetNexoBotState(stepContext);

            List<StudyCenterModel> centers = _openDataService.GetStudyCenters();
            state.StudyCenterModel = centers.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("AnswerCenterName", state.StudyCenterModel));
            var degrees = state.StudyCenterModel.Degrees.Select(x => x.Name.ToLower()).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("DegreePrefix", state.StudyCenterModel)),
                Choices = ChoiceFactory.ToChoices(degrees),
                RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskDegreeNameAgain", null))
            });
        }

        private async Task<DialogTurnResult> QuestionSubjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string degreeNameResponse = (stepContext.Result as FoundChoice)?.Value;

            var state = await GetNexoBotState(stepContext);
            state.DegreeCenterModel = state.StudyCenterModel.Degrees.FirstOrDefault(x => x.Name.ToLower().Contains(degreeNameResponse.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("AnswerDegreeName", state.DegreeCenterModel));

            var subjects = state.DegreeCenterModel.Subjects.Select(x => x.Name).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("SubjectPrefix",state.DegreeCenterModel)),
                Choices = ChoiceFactory.ToChoices(subjects),
                RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskSubjectAgain", null))
            });
        }

        private async Task<DialogTurnResult> EndQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string subjectNameResponse = (stepContext.Result as FoundChoice)?.Value;

            NexoBotState state = await GetNexoBotState(stepContext);
            state.SubjectModel = state.DegreeCenterModel.Subjects.FirstOrDefault(x => x.Name.ToLower().Contains(subjectNameResponse.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("AnswerSubjectName", state.SubjectModel));

            return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);

        }

    }
}
