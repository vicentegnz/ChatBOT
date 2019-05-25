﻿using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
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

        #endregion


        public QuestionDialog(string dialogId, BotServices services, ISpellCheckService spellCheckService  ,ISearchService searchService, IEnumerable<WaterfallStep> steps = null) : base(dialogId ?? nameof(QuestionDialog))
        {
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
                                        ActQuestionStepAsync,
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
                    message = $"No entiendo muy bien tu pregunta, he realizado una busqueda por la página de la Universidad de Extremadura y he encontrado esto:\n{searchResponse.Description}\n{searchResponse.Url}";
            }

            await stepContext.Context.SendActivityAsync(message);
            return await stepContext.NextAsync();
        }


        private async Task<DialogTurnResult> ActQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = stepContext.Context.Activity.CreateReply($"¿Te ha servido la respuesta? Si no te ha servido la respuesta vuelve a formularmela de otra manera.")
                });
        }


        private async Task<DialogTurnResult> RepeatQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await (stepContext.Context.TurnState[nameof(NexoBotAccessors)] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();

            if (state.Messages.Contains(stepContext.Result.ToString()))
            {
                await stepContext.Context.SendActivityAsync("Lo siento, pero no tengo la información que necesitas para esa pregunta.");
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
                    Prompt = stepContext.Context.Activity.CreateReply($"¿Necesitas hacer alguna otra pregunta?")
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
