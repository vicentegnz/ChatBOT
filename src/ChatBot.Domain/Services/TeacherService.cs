using ChatBot.Domain.Core;
using ChatBot.Domain.Extensions;
using ChatBot.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatBot.Domain.Services
{
    public class TeacherService : ITeacherService
    {
        private const string TEACHERS_PATH = "https://www3.unex.es/inf_academica_centro/index.php?mod=profesores&file=index&id_centro=16";

        public async Task<List<TeacherModel>> GetListOfTeachers()
        {
            List<TeacherModel> teachers = new List<TeacherModel>();
            var httpClient = new HttpClient();
            try { 
                var html = await httpClient.GetStringAsync(TEACHERS_PATH);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var teacherList = htmlDocument.DocumentNode.SelectNodes("//table/tr/td/a").ToList();

                foreach (var node in teacherList)
                {
                    var nameAndSurname = node.InnerHtml.RemoveDiacritics().Split(',');
                    var url = node.GetAttributeValue("onclick", string.Empty).Replace("javascript:window.top.location.href='", "");

                    teachers.Add(new TeacherModel
                    {
                        Name = $"{nameAndSurname[1]} {nameAndSurname[0]}",
                        InfoUrl = url
                    });
                }
            }
            catch(Exception)
            {
                return teachers;
            }

            return teachers;
        }
    }
}
