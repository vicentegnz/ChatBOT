using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public sealed class QuestionDialog : BaseDialog
    {

        #region "Properties"
        private readonly BotServices _services;
        private readonly ISpellCheckService _spellCheckService;
        private readonly ISearchService _searchService;
        private readonly TemplateEngine _lgEngine;
        
        #endregion


        public QuestionDialog(string dialogId, BotServices services, ISpellCheckService spellCheckService  ,ISearchService searchService, IEnumerable<WaterfallStep> steps = null) : base(dialogId ?? nameof(QuestionDialog))
        {
            string fullPath = Path.Combine(new string[] { ".", ".", "Resources", "QuestionDialog.lg" });
            _lgEngine = new TemplateEngine().AddFile(fullPath);

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
                    message = _lgEngine.EvaluateTemplate("Answer", searchResponse);
            }

            await stepContext.Context.SendActivityAsync(message);
            return await stepContext.NextAsync();
        }


        private async Task<DialogTurnResult> CheckIfAnswerIsValidStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskIfAnswerOk"))
                });
        }


        private async Task<DialogTurnResult> RepeatQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();

            if (state.Messages.Contains(stepContext.Result.ToString()))
            {
                await stepContext.Context.SendActivityAsync(_lgEngine.EvaluateTemplate("InfoNotFoundForSecondTime"));
                return await stepContext.NextAsync();
            }
            else
            {
                state.Messages.Add(stepContext.Result.ToString());
            }

            if (topIntent == null) return await stepContext.EndDialogAsync();

            return topIntent.Value.intent == LuisServiceConfiguration.OkIntent
                ? await stepContext.ReplaceDialogAsync(nameof(MainLuisDialog), null, cancellationToken)
                : await DialogByIntent(stepContext, topIntent);
        }


        private async Task<DialogTurnResult> IsValidQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply(_lgEngine.EvaluateTemplate("AskAgain"))
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
                : await DialogByIntent(stepContext, topIntent);


        }

    }
}
