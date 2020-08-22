using ChatBot.Entities;
using System.Collections.Generic;


namespace ChatBot.Domain.Services.OpenData
{
    public static class OpenDataInfoCache
    {

        private static List<StudyCenterModel> centersModel;

        public static List<StudyCenterModel> GetCentersModel()
        {
            return centersModel;
        }

        public static void SetCentersModel(List<StudyCenterModel> value)
        {
            centersModel = value;
        }
    }
}
