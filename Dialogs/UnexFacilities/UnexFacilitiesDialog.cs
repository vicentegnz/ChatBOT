﻿using ChatBOT.Core;
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

            ChoicePrompt choicePrompt = new ChoicePrompt(nameof(ChoicePrompt));
            choicePrompt.ChoiceOptions = new ChoiceFactoryOptions { IncludeNumbers = false };
            choicePrompt.RecognizerOptions = new FindChoicesOptions { AllowPartialMatches = true };
            AddDialog(choicePrompt);

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
              {
                                        IntroFacilitiesQuestionStepAsync,
                                        IntermediateFacilitiesAnswerStepAsync,                                   
                                        EndQuestionStepAsync,

              }));

            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> IntroFacilitiesQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<string> categoryFacilities = await _unexFacilitiesService.GetUnexFacilitiesCategories();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskFacilitieCategoryAgain")),
                    Choices = ChoiceFactory.ToChoices(categoryFacilities),
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskFacilitieCategory"))
                });
        }

        private async Task<DialogTurnResult> IntermediateFacilitiesAnswerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string category = (stepContext.Result as FoundChoice)?.Value;

            List<UnexFacilitieModel> facilities = await _unexFacilitiesService.GetUnexFacilities();

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    RetryPrompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskFacilitieAgain")),
                    Choices = ChoiceFactory.ToChoices(facilities.Where(x => x.Category.ToLower().Equals(category.ToLower())).Select(x => x.Name).Distinct().ToList()),
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskFacilitie"))
                });

        }



        private async Task<DialogTurnResult> EndQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var service = (stepContext.Result as FoundChoice)?.Value;
            var state = await GetNexoBotState(stepContext);
            List<UnexFacilitieModel> facilities = await _unexFacilitiesService.GetUnexFacilities();

            state.UnexFacilitieModel = facilities.FirstOrDefault(x => x.Name.ToLower().Contains(service.ToLower()));

            await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("AnswerFacilie", state.UnexFacilitieModel));

            return await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken);
        }

    }
}
