using Microsoft.ML.Runtime.Api;

namespace SentimentAnalysis.Models
{
    public class SentimentDataModel
    {
        [Column(ordinal: "0", name: "Label")]
        public float Sentiment;
        [Column(ordinal: "1")]
        public string SentimentText;
    }
}
