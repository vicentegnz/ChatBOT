using ChatBot.Services;
using ChatBOT.Bot;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Dialogs
{
    public sealed class QuestionDialog : WaterfallDialog
    {
        #region "Consts"
        private const string LuisKey = "HelpService";
        private const string QnaKey = "FrequentlyAskedQuestions";

        private const string UNKNOWN_INTENT_LUIS = "None";
        #endregion

        #region "Properties"
        private readonly BotServices _services;
        private readonly ISpellCheckService _spellCheck;
        private readonly ISearchService _searchService;

        #endregion


        public QuestionDialog(string dialogId,  BotServices services, ISpellCheckService spellCheck, ISearchService searchService, IEnumerable<WaterfallStep> steps = null) : base(dialogId, steps)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(LuisKey))
                throw new ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisKey}'.");

            if (!_services.QnAServices.ContainsKey(QnaKey))
                throw new ArgumentException($"La configuración no es correcta.Por favor comprueba que existe en tu fichero '.bot' un servicio Qna llamado '{QnaKey}'.");

            _searchService = searchService;

            //AddStep(async (stepContext, cancellationToken) =>
            //{
            //    var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
            //    var message = string.Empty;

            //    if (state.Messages.Any())
            //    {
            //        message = $"De acuerdo, puedes preguntar de nuevo lo que no hayas entendido.";
            //    }
            //    else
            //    {
            //        message = $"Has seleccionado otra consulta, puedes hacerme cualquier pregunta relacionada con la UEX, ten en cuenta que muchas de mis respuestas estarán basadas en el FAQ de la UNEX.";
            //    }


            //    return await stepContext.PromptAsync("textPrompt",
            //        new PromptOptions
            //        {
            //            Prompt = stepContext.Context.Activity.CreateReply(message)
            //        });
            //});


            AddStep(async (stepContext, cancellationToken) =>
            {
                var message = string.Empty;
                var state = await (stepContext.Context.TurnState["NexoBotAccessors"] as NexoBotAccessors).NexoBotStateStateAccessor.GetAsync(stepContext.Context);
                if(stepContext.Result != null)
                    state.Messages.Add(stepContext.Result.ToString());

                //Qna
                var response = await _services.QnAServices[QnaKey].GetAnswersAsync(stepContext.Context);
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
                var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(stepContext.Context, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();

                var result = stepContext.Result.ToString();
                if (topIntent.Value.intent == "Afirmacion")
                    return await stepContext.ReplaceDialogAsync(MainLuisDialog.Id, cancellationToken);
                else
                    return await stepContext.EndDialogAsync();
            });


        }

        public static string Id => "QuestionDialog";
    }
}
