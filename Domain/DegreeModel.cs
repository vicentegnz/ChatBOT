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
        public int Code { get; set; }
        public string Name { get; set; }
        public string Area { get; set; }
        public int Ects { get; set; }
        public int Semester { get; set; }
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
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<SubjectModel> Subjects {get; set;}
        public string Departament { get; set; }
        public string InfoUrl { get; set; }

    }
}
