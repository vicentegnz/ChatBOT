using System.Collections.Generic;

namespace ChatBot.Domain.Services.OpenData.Conf
{
    public class DegreeConfigModel
    {
        public string JsonDegreeUrl { get; set; }
        public string CentersQuery { get; set; }
        public string DegreesQuery { get; set; }
        public string SubjectsQuery { get; set; }
        public List<StudyCenterConfigModel> StudyCentres { get; set; }
        public string PathAbsoluteFrom12A { get; set; }
        public string PathAbsoluteCenter { get; set; }
        public string PathAbsoluteDegree { get; set; }
    }


    public class StudyCenterConfigModel
    {
        public int OpenDataCode { get; set; }
        public string UnexPageCode { get; set; }
    }
}
