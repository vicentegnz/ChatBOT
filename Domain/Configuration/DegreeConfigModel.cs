using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Domain
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
