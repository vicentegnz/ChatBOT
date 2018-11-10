using Microsoft.ML.Runtime.Api;

namespace SentimentAnalysis.Models
{
    public class SentimentPredictionModel
    {
        [ColumnName("PredictedLabel")]
        public bool Sentiment;
    }
}
