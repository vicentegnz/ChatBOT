using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Domain
{
    public class DegreeModel
    {

        public string Name { get; set; }

        public string Code { get; set; }

        public List<SubjectModel> Subjects { get; set; }

        public List<ExamModel> Exams { get; set; }

        public List<TeacherModel> Teachers { get; set; }

    }

    public class SubjectModel
    {
        public string Name { get; set; }
        public string InfoUrl { get; set; }

    }
    public class ExamModel
    {
        public int Semestre { get; set; }
        public string InfoUrl { get; set; }

    }

    public class TeacherModel
    {
        public string Name { get; set; }
        public string InfoUrl { get; set; }

    }
}
