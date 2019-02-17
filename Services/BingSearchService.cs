using ChatBOT.Core;
using ChatBOT.Domain;
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Services
{
    public class BingSearchService : ISearchService
    {
        private const string API_KEY = "5528fa331bbb464fad299933388be24a";

        private readonly WebSearchClient _webSearchClient;

        public BingSearchService()
        {
            _webSearchClient = new WebSearchClient(new ApiKeyServiceClientCredentials(API_KEY));
        }


        public async Task<SearchResponseModel> GetResultFromSearch(string messageToSearch)
        {
            SearchResponseModel searchResponse = new SearchResponseModel();
            messageToSearch = messageToSearch + " site:unex.es";

            var webData = await _webSearchClient.Web.SearchAsync(query: messageToSearch, market: "es-ES", safeSearch: "strict");

            if (webData?.WebPages?.Value?.Count > 0)
            {
                // find the first web page
                var firstWebPagesResult = webData.WebPages.Value.FirstOrDefault();

                if (firstWebPagesResult != null)
                {
                    searchResponse.Title = firstWebPagesResult.Name;
                    searchResponse.Url = firstWebPagesResult.Url;
                    searchResponse.Description = firstWebPagesResult.Snippet;
                }
            }

            return searchResponse;

        }
    }
}
