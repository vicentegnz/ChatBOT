using ChatBot.Domain.Bot;
using ChatBot.Domain.Configuration;
using ChatBot.Domain.Core;
using ChatBot.Domain.Services.IA;
using ChatBot.Entities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Domain.Dialogs.Question
{
    public sealed class QuestionDialog : BaseDialog
    {

        #region "Properties"
        private readonly BotServices _services;
        private readonly ISpellCheckService _spellCheckService;
        private readonly ISearchService _searchService;
        private readonly Templates _lgEngine;
        
        #endregion


        public QuestionDialog(string dialogId, BotServices services, ISpellCheckService spellCheckService  ,ISearchService searchService, IEnumerable<WaterfallStep> steps = null) : base(dialogId ?? nameof(QuestionDialog))
        {
            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "QuestionDialog.lg" });
            _lgEngine = Templates.ParseFile(fullPath);

            _services = services ?? throw new ArgumentNullException(nameof(services));

            if (!_services.LuisServices.ContainsKey(LuisServiceConfiguration.LuisKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisServiceConfiguration.LuisKey}'.");

            if (!_services.QnAServices.ContainsKey(QnaMakerServiceConfiguration.QnaKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio Qna llamado '{QnaMakerServiceConfiguration.QnaKey}'.");

            _searchService = searchService;
            _spellCheckService = spellCheckService;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                                        IntroQuestionStepAsync,
                                        CheckIfAnswerIsValidStepAsync,
                                        RepeatQuestionStepAsync,
                                        IsValidQuestionStepAsync,
                                        EndQuestionStepAsync

            }));

            InitialDialogId = nameof(WaterfallDialog);

        }
        private async Task<DialogTurnResult> IntroQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = string.Empty;
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);

            if (stepContext.Result != null)
                state.Messages.Add(stepContext.Result.ToString());

            //Qna
            var response = await _services.QnAServices[QnaMakerServiceConfiguration.QnaKey].GetAnswersAsync(stepContext.Context);

            if (response != null && response.Length > 0)
                message = response[0].Answer;
            else
            {
                //Bing Web Search
                SearchResponseModel searchResponse = await _searchService.GetResultFromSearch(stepContext.Context.Activity.Text);
                if (searchResponse != null)
                    message = _lgEngine.Evaluate("Answer", searchResponse).ToString();
            }

            await stepContext.Context.SendActivityAsync(message);
            return await stepContext.NextAsync();
        }


        private async Task<DialogTurnResult> CheckIfAnswerIsValidStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            Thread.Sleep(3000);

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("AskIfAnswerOk").ToString())
                });
        }


        private async Task<DialogTurnResult> RepeatQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();

            if (state.Messages.Contains(stepContext.Result.ToString()))
            {
                await stepContext.Context.SendActivityAsync(_lgEngine.Evaluate("InfoNotFoundForSecondTime").ToString());
                return await stepContext.NextAsync();
            }
            else
            {
                state.Messages.Add(stepContext.Result.ToString());
            }

            if (topIntent == null) return await stepContext.EndDialogAsync();

            return topIntent.Value.intent == LuisServiceConfiguration.OkIntent
                ? await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken)
                : await BeginDialogByIntent(stepContext, topIntent);
        }


        private async Task<DialogTurnResult> IsValidQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Thread.Sleep(3000);

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.Evaluate("AskAgain").ToString())
                });
        }     

        private async Task<DialogTurnResult> EndQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();

            state.Messages.Add(stepContext.Result.ToString());

            if (topIntent == null) return await stepContext.EndDialogAsync();

            return topIntent.Value.intent == LuisServiceConfiguration.OkIntent
                ? await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken)
                : await BeginDialogByIntent(stepContext, topIntent);


        }

    }
}
