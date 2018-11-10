using AutoMapper;
using ChatBOT.Models;
using SentimentAnalysis.Models;

namespace ChatBOT.Profiles
{
    public class SentimentAnalysisProfile : Profile
    {

        public SentimentAnalysisProfile()
        {
            CreateMap<SentimentDataModelView, SentimentDataModel>();
            CreateMap<SentimentPredictionModelView, SentimentPredictionModel>();
        }

    }
}
