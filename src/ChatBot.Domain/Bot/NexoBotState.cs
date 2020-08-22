using ChatBot.Domain;
using ChatBot.Entities;
using System.Collections.Generic;

namespace ChatBot.Domain.Bot
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
