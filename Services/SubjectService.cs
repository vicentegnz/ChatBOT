using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Services
{
    public class SubjectService : ISubjectService
    {
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
                result = client.GetStringAsync(_degreeConfigModel.JsonStudyCenterUrl)?.Result ?? string.Empty; //uri
            }

            if (string.IsNullOrEmpty(result))
                return centersToReturn;

            var degrees = (JArray)JObject.Parse(result)["results"]["bindings"];

            foreach (var degree in degrees)
            {
                var unexCode = _degreeConfigModel.StudyCentres.FirstOrDefault(x => x.OpenDataCode == (int)degree["aiiso_code"]["value"])?.UnexPageCode ?? string.Empty;

                if (!string.IsNullOrEmpty(unexCode))
                    centersToReturn.Add(new StudyCenterModel
                    {
                        Code = (int)degree["aiiso_code"]["value"],
                        Name = (string)degree["foaf_name"]["value"],
                        UnexCode = unexCode

                    });

            }

            return centersToReturn;
        }


        public  List<DegreeModel> GetDegrees()
        {
            var result = string.Empty;
            var degreesToReturn = new List<DegreeModel>();

            using (var client = new System.Net.Http.HttpClient())
            {
                result = client.GetStringAsync(_degreeConfigModel.JsonDegreeUrl)?.Result ?? string.Empty; //uri
            }

            if (string.IsNullOrEmpty(result))
                 return degreesToReturn;

            var degrees = (JArray)JObject.Parse(result)["results"]["bindings"];
            
            foreach (var degree in degrees)
            {
                degreesToReturn.Add(new DegreeModel
                {
                    Code = (int)degree["aiiso_code"]["value"],
                    Name = (string)degree["foaf_name"]["value"],
                    

                });

            }

            return degreesToReturn;
        }

        public List<SubjectModel> GetSubjectbyDegreeCode()
        {
            return new List<SubjectModel>();
        }



    }

}
