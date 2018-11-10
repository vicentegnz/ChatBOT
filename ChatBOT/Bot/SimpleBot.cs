using System.Threading;
using System.Threading.Tasks;
using ChatBOT.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ChatBOT.Bot
{
    public class SimpleBot : IBot
    {

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                var sentimentAnalysisResult = (SentimentPredictionModelView)turnContext.TurnState["SentimentPrediction"];

                var result = sentimentAnalysisResult.Sentiment ? "Positive" : "Negative";

                await turnContext.SendActivityAsync($"You said {turnContext.Activity.Text}, the sentiment according to the middleware is {result}");
            }
        }
        
    }
}
