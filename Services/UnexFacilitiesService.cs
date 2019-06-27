using ChatBOT.Core;
using ChatBOT.Core.Extensions;
using ChatBOT.Domain;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatBOT.Services
{
    public class UnexFacilitiesService : IUnexFacilitiesService
    {

        #region Consts

        private const string UNEX_FACILITIES_PATH = "https://www.unex.es/organizacion/servicios-universitarios";

        #endregion

        public async Task<List<UnexFacilitieModel>> GetUnexFacilities()
        {
            List<UnexFacilitieModel> unexFacilitiesModels = new List<UnexFacilitieModel>();
            var httpClient = new HttpClient();
            try
            {
                var html = await httpClient.GetStringAsync(UNEX_FACILITIES_PATH);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var facilitiesList = htmlDocument.DocumentNode.SelectNodes("//div/dl/dt/span/a").ToList();

                foreach (var node in facilitiesList)    
                {
                    unexFacilitiesModels.Add(new UnexFacilitieModel
                    {
                        Name = node.InnerHtml,
                        Url = node.GetAttributeValue("href", string.Empty)
                    });
                }
            }
            catch (Exception)
            {
                return unexFacilitiesModels;
            }

            return unexFacilitiesModels;
        }
    }
}
