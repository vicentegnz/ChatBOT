using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Conf;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace ChatBOT.Dialogs
{
    public sealed class QuestionDialog : BaseDialog
    {

        #region "Properties"
        private readonly BotServices _services;
        private readonly ISpellCheckService _spellCheckService;
        private readonly ISearchService _searchService;

        #endregion


        public QuestionDialog(string dialogId,  BotServices services,ISpellCheckService spellCheckService  ,ISearchService searchService, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(LuisServiceConfiguration.LuisKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisServiceConfiguration.LuisKey}'.");

            if (!_services.QnAServices.ContainsKey(QnaMakerServiceConfiguration.QnaKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio Qna llamado '{QnaMakerServiceConfiguration.QnaKey}'.");

            _searchService = searchService;
            _spellCheckService = spellCheckService;
            
            AddStep(async (stepContext, cancellationToken) =>
            {
                var message = string.Empty;
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);

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
                        message = $"{searchResponse.Description}\n{searchResponse.Url}";
                }
                  
                await stepContext.Context.SendActivityAsync(message);
                return await stepContext.NextAsync();
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                return await stepContext.PromptAsync("textPrompt",
                    new PromptOptions
                    {
                        Prompt = stepContext.Context.Activity.CreateReply($"¿Necesitas hacer alguna otra pregunta?")
                    });
            });

            AddStep(async (stepContext, cancellationToken) =>
            {
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                var recognizerResult = await _services.LuisServices[LuisServiceConfiguration.LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();

                state.Messages.Add(stepContext.Result.ToString());

                if (topIntent == null) return await stepContext.EndDialogAsync();

                return topIntent.Value.intent == LuisServiceConfiguration.OkIntent
                    ? await stepContext.ReplaceDialogAsync(MainLuisDialog.Id, cancellationToken)
                    : await DialogByIntent(stepContext, topIntent);


            });
            
        }

        public new static string Id => "QuestionDialog";
    }
}
