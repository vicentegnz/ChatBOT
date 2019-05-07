using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBOT.Services
{
    public class SubjectService : ISubjectService
    {

        private const string TAG_VALUE = "value";
        private const string TAG_RESULTS = "results";
        private const string TAG_BINDINGS = "bindings";


        private const string TAG_CODE = "aiiso_code";
        private const string TAG_NAME = "foaf_name";
        private const string TAG_URL_CENTER = "schema_url";
        private const string TAG_URL_DEGREE = "uri";
        private const string TAG_EMAIL = "schema_email";
        private const string TAG_TELEPHONE = "schema_telephone";
        private const string TAG_EXTINCION = "ou_cursoInicioExtincion";
        private const string TAG_CENTRO_IMPARTIDA = "ou_impartidaEnCentro";
        private const string TAG_ASIGNATURAS = "tieneAsignaturas";


        protected readonly DegreeConfigModel _degreeConfigModel;

        public SubjectService(IOptions<DegreeConfigModel> degreeConfigModel)
        {
            _degreeConfigModel = degreeConfigModel.Value;
        }

        public List<StudyCenterModel> GetStudyCenters()
        {
            var result = string.Empty;
            var centersToReturn = new List<StudyCenterModel>();

            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(_degreeConfigModel.JsonStudyCenterUrl)?.Result ?? string.Empty; 
            }

            if (string.IsNullOrEmpty(result))
                return centersToReturn;

            var degrees = (JArray)JObject.Parse(result)[TAG_RESULTS][TAG_BINDINGS];

            foreach (var degree in degrees)
            {
                var unexCode = _degreeConfigModel.StudyCentres.FirstOrDefault(x => x.OpenDataCode == (int)degree[TAG_CODE][TAG_VALUE])?.UnexPageCode ?? string.Empty;

                if (string.IsNullOrEmpty(unexCode)) continue;

                centersToReturn.Add(new StudyCenterModel
                {
                    Code = (int)degree[TAG_CODE][TAG_VALUE],
                    Name = (string)degree[TAG_NAME][TAG_VALUE],
                    UnexCode = unexCode,
                    Url = (string)degree[TAG_URL_CENTER][TAG_VALUE],
                    Telephone = (string)degree[TAG_TELEPHONE][TAG_VALUE],
                    Email = (string)degree[TAG_EMAIL][TAG_VALUE]
                });
            }
            return centersToReturn;
        }


        public  List<DegreeModel> GetDegrees()
        {
            var result = string.Empty;
            var centersToReturn = new List<DegreeModel>();

            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(_degreeConfigModel.JsonDegreeUrl)?.Result ?? string.Empty;
            }

            if (string.IsNullOrEmpty(result))
                return centersToReturn;

            var degrees = (JArray)JObject.Parse(result)[TAG_RESULTS][TAG_BINDINGS];

            foreach (var degree in degrees)
            {
                //var extincion = (string)degree[TAG_EXTINCION][TAG_VALUE] ?? string.Empty;

                var subjects = degree[TAG_ASIGNATURAS][TAG_VALUE]?.ToString()?.Split(" ", 
                    StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();

                if (!subjects.Any() /*|| !string.IsNullOrEmpty(extincion)*/) continue;

                centersToReturn.Add(new DegreeModel
                {
                    Code = (int)degree[TAG_CODE][TAG_VALUE],
                    Name = (string)degree[TAG_NAME][TAG_VALUE],
                    Url = (string)degree[TAG_URL_DEGREE][TAG_VALUE],
                    Center = GetStudyCenter((string)degree[TAG_CENTRO_IMPARTIDA][TAG_VALUE]),
                    Subjects = GetSubjectsFromListOfUrisFromOpenData(subjects)
                });
            }
            return centersToReturn;
        }

        private string GetStudyCenter(string uriOpenDataCenter)
        {
            var result = string.Empty;

            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(uriOpenDataCenter)?.Result ?? string.Empty;
            }

            return result;
        }

        private List<SubjectModel>  GetSubjectsFromListOfUrisFromOpenData(List<string> urisFromOpenData)
        {
            return new List<SubjectModel>();
        }

        public List<SubjectModel> GetSubjectbyDegreeCode()
        {
            return new List<SubjectModel>();
        }



    }

}
