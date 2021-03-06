﻿using System.Collections.Generic;

namespace ChatBot.Entities
{
    public class DegreeModel
    {

        public string Name { get; set; }

        public int Code { get; set; }

        public string Url { get; set; }

        public string Center { get; set; }

        public List<SubjectModel> Subjects { get; set; }

        public List<TeacherModel> Teachers { get; set; }

    }

    public class StudyCenterModel
    {

        public string Name { get; set; }

        public string UnexCode { get; set; }

        public int Code { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public string Url { get; set; }

        public List<DegreeModel> Degrees { get; set; }

    }

    public class SubjectModel
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public string Caracter { get; set; }
        public int Ects { get; set; }
        public string Semester { get; set; }
        public string InfoUrl { get; set; }
        public string Degree { get; set; }
        public int Students { get; set; }

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
