using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services;
using ChatBOT.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class NexoBot : IBot
    {
        public static readonly string LuisKey = "HelpService";
        private const string WelcomeText = "This bot will introduce you to natural language processing with LUIS. Type an utterance to get started";

        private readonly BotServices _services;

    
        public NexoBot(BotServices services)
        {
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(LuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{LuisKey}'.");
            }
        }


        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

            switch (turnContext.Activity.Type) {

                case ActivityTypes.Message:

                    
                    //LUIS
                    var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();

                    if (topIntent != null && topIntent.HasValue && topIntent.Value.intent != "None")
                    {
                        await turnContext.SendActivityAsync($"LUIS dice que el intent con mayor puntuacion para el mensaje {turnContext.Activity.Text} es {topIntent.Value.intent}, con una puntuación de {topIntent.Value.score}\n");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync($"No existe ningún intent para el mensaje {turnContext.Activity.Text}.");
                    }

                    //SentimentAnalitys
                    //var sentimentAnalysisResult = (SentimentPredictionModelView)turnContext.TurnState["SentimentPrediction"];

                    //var result = sentimentAnalysisResult.Sentiment ? "Positive" : "Negative";

                    //await turnContext.SendActivityAsync($"You said {turnContext.Activity.Text}, the sentiment according to the middleware is {result}");

                    break;
                case ActivityTypes.ConversationUpdate:
                    break;


                default:
                    break;

            }
        }
        
    }
}
