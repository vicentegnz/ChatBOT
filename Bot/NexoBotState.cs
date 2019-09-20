using ChatBOT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Bot
{
    public class NexoBotState
    {
        public List<string> Messages { get; set; }

        public StudyCenterModel StudyCenterModel { get; set; }

        public DegreeModel DegreeCenterModel { get; set; }

        public SubjectModel SubjectModel { get; set; }

        public UnexFacilitieModel UnexFacilitieModel { get; set; }
        
        public TeacherModel TeacherModel { get; set; }

        public NexoBotState()
        {
            Messages = new List<string>();
            StudyCenterModel = new StudyCenterModel();
            DegreeCenterModel = new DegreeModel();
            SubjectModel = new SubjectModel();
            TeacherModel = new TeacherModel();
            UnexFacilitieModel = new UnexFacilitieModel();

        }
    }
}
