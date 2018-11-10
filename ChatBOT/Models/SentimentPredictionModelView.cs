using Microsoft.ML.Runtime.Api;

namespace ChatBOT.Models
{
    public class SentimentPredictionModelView
    {
        [ColumnName("PredictedLabel")]
        public bool Sentiment;
    }
}
