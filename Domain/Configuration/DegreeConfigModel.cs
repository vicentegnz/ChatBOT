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
        public List<StudyCentreConfigModel> StudyCentres { get; set; }
        public string PathAbsoluteFrom12A { get; set; } 
    }


    public class StudyCentreConfigModel
    {
        public int OpenDataCode { get; set; }
        public string UnexPageCode { get; set; }
    }
}
