using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.ML.Legacy;
using SentimentAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Middleware
{
    public class SentimentAnalysisMiddleware : IMiddleware
    {
        private PredictionModel<SentimentData, SentimentPrediction> model;

        public SentimentAnalysisMiddleware()
        {

        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                if (model == null)
                {
                    model = await PredictionModel.ReadAsync<SentimentData, SentimentPrediction>("Model.zip");
                }

                var predictedSentiment = model.Predict(new SentimentData() { SentimentText = context.Activity.Text });
                context.TurnState.Add("SentimentPrediction", new SentimentPrediction() { Sentiment = predictedSentiment.Sentiment });
            }

            await next(cancellationToken);
        }
    }
}