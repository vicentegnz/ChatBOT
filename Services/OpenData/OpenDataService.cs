using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBOT.Services
{
    public class OpenDataService : IOpenDataService
    {
        //COMUN
        private const string TAG_VALUE = "value";
        private const string TAG_RESULTS = "results";
        private const string TAG_BINDINGS = "bindings";

        //CENTERS
        private const string TAG_CENTER_CODE = "ou_codCentro";
        private const string TAG_CENTER_NAME = "foaf_name";
        private const string TAG_CENTER_EMAIL = "schema_email";
        private const string TAG_CENTER_TELEPHONE = "schema_telephone";

        //DEGREES
        private const string TAG_DEGREE_NAME = "foaf_name";
        private const string TAG_DEGREE_CODE = "ou_codTitulacion";
        private const string TAG_DEGREE_CENTER = "foaf_name_centro";

        //SUBJECTS
        private const string TAG_SUBJECT_CODE = "aiiso_code";
        private const string TAG_SUBJECT_NAME = "teach_courseTitle";
        private const string TAG_SUBJECT_DEGREE = "titulacion";
        private const string TAG_SUBJECT_ECTS = "teach_ects";
        private const string TAG_SUBJECT_CHARACTER = "ou_caracterAsignatura";
        private const string TAG_SUBJECT_TIME = "ou_temporalidadAsignatura";

        protected readonly DegreeConfigModel _degreeConfigModel;

        public OpenDataService(IOptions<DegreeConfigModel> degreeConfigModel)
        {
            _degreeConfigModel = degreeConfigModel.Value;
        }

        public List<StudyCenterModel> GetStudyCenters()
        {
            var result = string.Empty;
          
            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(_degreeConfigModel.CentersQuery)?.Result ?? string.Empty;
            }

            if (string.IsNullOrEmpty(result))
                return new List<StudyCenterModel>();

            List<StudyCenterModel> centers = GetCenters(result);

            var subjects = GetSubjects();
            foreach (var degree in GetDegrees())
            {
                var center = centers.FirstOrDefault(x => x.Name.Equals(degree.Center));
                if(center != null) {
                    degree.Url = string.Format(_degreeConfigModel.PathAbsoluteDegree, center.UnexCode, degree.Code.ToString("D4"));
                    degree.Subjects.AddRange(subjects
                        .Where(x => x.Degree.Equals(degree.Name))
                        .Select(c => { c.InfoUrl = string.Format(_degreeConfigModel.PathAbsoluteFrom12A, center.UnexCode, degree.Code.ToString("D4"), c.Code); return c; }));
                    center.Degrees.Add(degree);
                }
            }

            return centers;
        }

        private List<StudyCenterModel> GetCenters(string result)
        {
            var jsonCenters = (JArray)JObject.Parse(result)[TAG_RESULTS][TAG_BINDINGS];
            var centers = new List<StudyCenterModel>();

            foreach (var center in jsonCenters)
            {
                var unexCode = _degreeConfigModel.StudyCentres.FirstOrDefault(x => x.OpenDataCode == (int)center[TAG_CENTER_CODE][TAG_VALUE])?.UnexPageCode ?? string.Empty;

                if (string.IsNullOrEmpty(unexCode)) continue;

                centers.Add(new StudyCenterModel
                {
                    Code = (int)center[TAG_CENTER_CODE][TAG_VALUE],
                    Name = (string)center[TAG_CENTER_NAME][TAG_VALUE],
                    UnexCode = unexCode,
                    Url = string.Format(_degreeConfigModel.PathAbsoluteCenter, unexCode),
                    Telephone = (string)center[TAG_CENTER_TELEPHONE][TAG_VALUE],
                    Email = (string)center[TAG_CENTER_EMAIL][TAG_VALUE],
                    Degrees = new List<DegreeModel>()
                });
            }

            return centers;
        }
        private  List<DegreeModel> GetDegrees()
        {
            var result = string.Empty;
            
            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(_degreeConfigModel.DegreesQuery)?.Result ?? string.Empty;
            }

            if (string.IsNullOrEmpty(result))
                return new List<DegreeModel>();

            return GetDegreesFromJson(result);
        }
        private List<SubjectModel> GetSubjects()
        {
            var result = string.Empty;

            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(_degreeConfigModel.SubjectsQuery)?.Result ?? string.Empty;
            }

            if (string.IsNullOrEmpty(result))
                return new List<SubjectModel>();

            return GetSubjectsFromJson(result);

        }

        
        private List<DegreeModel> GetDegreesFromJson(string result)
        {
            var jsonDegrees = (JArray)JObject.Parse(result)[TAG_RESULTS][TAG_BINDINGS];
            var degrees = new List<DegreeModel>();
            foreach (var degree in jsonDegrees)
            {
                var centro = degree[TAG_DEGREE_CENTER]?[TAG_VALUE] ?? string.Empty;

                degrees.Add(new DegreeModel
                {
                    Code = (int)degree[TAG_DEGREE_CODE][TAG_VALUE],
                    Name = (string)degree[TAG_DEGREE_NAME][TAG_VALUE],
                    Center = (string)centro,
                    Subjects = new List<SubjectModel>()
                });
            }

            return degrees;
        }
        private List<SubjectModel> GetSubjectsFromJson(string result)
        {
            var jsonSubjects = (JArray)JObject.Parse(result)[TAG_RESULTS][TAG_BINDINGS];
            var subjects = new List<SubjectModel>();

            foreach (var subject in jsonSubjects)
            {
                subjects.Add(new SubjectModel
                {
                    Code = (int)subject[TAG_SUBJECT_CODE][TAG_VALUE],
                    Name = (string)subject[TAG_SUBJECT_NAME][TAG_VALUE],
                    Degree = (string)subject[TAG_SUBJECT_DEGREE][TAG_VALUE],
                    Ects = (int)subject[TAG_SUBJECT_ECTS][TAG_VALUE],
                    //Semester = (string)subject[TAG_SUBJECT_TIME][TAG_VALUE],
                    Caracter = (string)subject[TAG_SUBJECT_CHARACTER][TAG_VALUE],
                    //Students = (int)subject[TAG_SUBJECT_STUDENTS][TAG_VALUE]
                });
            }
            return subjects;
        }
    }

}
