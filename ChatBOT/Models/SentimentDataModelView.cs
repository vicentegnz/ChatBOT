using Microsoft.ML.Runtime.Api;

namespace ChatBOT.Models
{
    public class SentimentDataModelView
    {
        [Column(ordinal: "0", name: "Label")]
        public float Sentiment { get; set; }
        [Column(ordinal: "1")]
        public string SentimentText { get; set; }

    }
}
