using ChatBOT.Domain;
using System.Collections.Generic;


namespace ChatBOT.Services.OpenData
{
    public static class CenterOpenDataInfo
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
