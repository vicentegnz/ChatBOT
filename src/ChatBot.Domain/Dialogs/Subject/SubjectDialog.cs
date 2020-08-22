

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Domain.Bot;
using ChatBot.Domain.Core;
using ChatBot.Entities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace ChatBot.Domain.Dialogs.Subject
{
    public sealed class SubjectDialog : BaseDialog
    {
        #region Properties

        private readonly IOpenDataService _openDataService;
        private readonly Templates _lgEngine;

        #endregion

        public SubjectDialog(string dialogId, IOpenDataService openDataService, IEnumerable<WaterfallStep> steps = null) : base(dialogId)
        {
            
            _openDataService = openDataService;

            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "SubjectDialog.lg" });
            _lgEngine = Templates.ParseFile(fullPath);

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

            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("InitSubjectDialog").ToString());

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("AskCenterName").ToString()),
                Choices = ChoiceFactory.ToChoices(centers.Select(x => x.Name).ToList()),
                RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("AskCenterNameAgain").ToString())
            });
        }

        private async Task<DialogTurnResult> QuestionDegreeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = (stepContext.Result as FoundChoice)?.Value;
            var state = await GetNexoBotState(stepContext);

            List<StudyCenterModel> centers = _openDataService.GetStudyCenters();
            state.StudyCenterModel = centers.FirstOrDefault(x => x.Name.ToLower().Contains(response.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("AnswerCenterName", state.StudyCenterModel).ToString());
            var degrees = state.StudyCenterModel.Degrees.Select(x => x.Name.ToLower()).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("DegreePrefix", state.StudyCenterModel).ToString()),
                Choices = ChoiceFactory.ToChoices(degrees),
                RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("AskDegreeNameAgain").ToString())
            });
        }

        private async Task<DialogTurnResult> QuestionSubjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string degreeNameResponse = (stepContext.Result as FoundChoice)?.Value;

            var state = await GetNexoBotState(stepContext);
            state.DegreeCenterModel = state.StudyCenterModel.Degrees.FirstOrDefault(x => x.Name.ToLower().Contains(degreeNameResponse.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("AnswerDegreeName", state.DegreeCenterModel).ToString());

            var subjects = state.DegreeCenterModel.Subjects.Select(x => x.Name).ToList();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("SubjectPrefix",state.DegreeCenterModel).ToString()),
                Choices = ChoiceFactory.ToChoices(subjects),
                RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("AskSubjectAgain").ToString())
            });
        }

        private async Task<DialogTurnResult> EndQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string subjectNameResponse = (stepContext.Result as FoundChoice)?.Value;

            NexoBotState state = await GetNexoBotState(stepContext);
            state.SubjectModel = state.DegreeCenterModel.Subjects.FirstOrDefault(x => x.Name.ToLower().Contains(subjectNameResponse.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("AnswerSubjectName", state.SubjectModel).ToString());

            return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);

        }

    }
}
