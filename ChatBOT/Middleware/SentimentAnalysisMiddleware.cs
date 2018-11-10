using ChatBOT.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.ML.Legacy;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBOT.Middleware
{
    public class SentimentAnalysisMiddleware : IMiddleware
    {
        private PredictionModel<SentimentDataModelView, SentimentPredictionModelView> model;
        
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                if (model == null)
                {
                    model = await PredictionModel.ReadAsync<SentimentDataModelView, SentimentPredictionModelView>("Model.zip");
                }

                var predictedSentiment = model.Predict(new SentimentDataModelView() { SentimentText = context.Activity.Text });
                context.TurnState.Add("SentimentPrediction", new SentimentPredictionModelView() { Sentiment = predictedSentiment.Sentiment });
            }

            await next(cancellationToken);
        }
    }
}