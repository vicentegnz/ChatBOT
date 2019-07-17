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

                var categoriesFacilitiesService = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='content-core']");
                var categoryName = "Servicio";

                foreach (var servicesTypeNode in categoriesFacilitiesService?.ChildNodes)    
                {
                    if (servicesTypeNode.OriginalName.Equals("h2"))
                        categoryName = servicesTypeNode.SelectSingleNode("span/a").InnerHtml;

                    if (servicesTypeNode.OriginalName.Equals("dl"))
                    {
                        var facilitie = servicesTypeNode.SelectSingleNode("dt/span/a");
                        unexFacilitiesModels.Add(new UnexFacilitieModel
                        {
                            Category = categoryName,
                            Name = facilitie.InnerHtml,
                            Url = facilitie.GetAttributeValue("href", string.Empty)
                        });

                    }
                }
            }
            catch (Exception)
            {
                return unexFacilitiesModels;
            }

            return unexFacilitiesModels;
        }

        public async Task<List<string>> GetUnexFacilitiesCategories()
        {
            var categories = new List<string>();
            var httpClient = new HttpClient();
            try
            {
                var html = await httpClient.GetStringAsync(UNEX_FACILITIES_PATH);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var categoriesFacilities = htmlDocument.DocumentNode.SelectNodes("//div[@id='content-core']/h2/span/a").ToList();

                foreach (var category in categoriesFacilities) categories.Add(category.InnerHtml);

            }
            catch (Exception)
            {
                return categories;
            }

            return categories;
        }
    }
}
