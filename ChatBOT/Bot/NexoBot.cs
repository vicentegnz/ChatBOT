﻿using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class NexoBot : IBot
    {
        public static readonly string LuisKey = "HelpService";
        public static readonly string QnaKey = "FrequentlyAskedQuestions";

        private const string WelcomeText = "¿te puedo ayudar en algo?";

        private readonly BotServices _services;
        private readonly ISpellCheckService _spellCheck;
        private readonly ISearchService _searchService;


        public NexoBot(BotServices services, ISpellCheckService spellCheck, ISearchService searchService)
        {
            _services = services ?? throw new System.ArgumentNullException(nameof(services));

            if (!_services.LuisServices.ContainsKey(LuisKey))
            {
                throw new System.ArgumentException($"La configuración no es correcta. Por favor comprueba que existe en tu fichero '.bot' un servicio LUIS llamado '{LuisKey}'.");
            }

            if (!_services.QnAServices.ContainsKey(QnaKey))
            {
                throw new System.ArgumentException($"La configuración no es correcta.Por favor comprueba que existe en tu fichero '.bot' un servicio Qna llamado '{QnaKey}'.");
            }

            _spellCheck = spellCheck;
            _searchService = searchService;
        }


        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

            switch (turnContext.Activity.Type) {

                case ActivityTypes.Message:

                    //turnContext.Activity.Text = _spellCheck.GetSpellCheckFromMessage(turnContext.Activity.Text);

                    //Qna
                    var response = await _services.QnAServices[QnaKey].GetAnswersAsync(turnContext);

                    if (response != null && response.Length > 0)
                    {
                        await turnContext.SendActivityAsync(response[0].Answer, cancellationToken: cancellationToken);
                    }
                    else {
                        //LUIS
                        var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(turnContext, cancellationToken);
                        var topIntent = recognizerResult?.GetTopScoringIntent();

                        if (topIntent != null && topIntent.HasValue && topIntent.Value.intent != "None")
                        {
                            await turnContext.SendActivityAsync($"LUIS dice que el intent con mayor puntuacion para el mensaje {turnContext.Activity.Text} es {topIntent.Value.intent}, con una puntuación de {topIntent.Value.score}\n");
                        }
                        else
                        {
                            SearchResponseModel searchResponse = await _searchService.GetResultFromSearch(turnContext.Activity.Text);

                            if (searchResponse != null)
                            {
                                await turnContext.SendActivityAsync($"{searchResponse.Description}\n{searchResponse.Url}");
                            }
                        }
                        
                    }



                    break;
                case ActivityTypes.ConversationUpdate:

                    if (turnContext.Activity.MembersAdded != null)
                    {
                        await SendWelcomeMessageAsync(turnContext, cancellationToken);
                    }

                    break;


                default:
                    break;

            }
        }


        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Hola {member.Name}, mi nombre es Nexo {WelcomeText}",
                        cancellationToken: cancellationToken);
                }
            }
        }

    }
}