
using ChatBOT.Domain;
using System.Threading.Tasks;

namespace ChatBOT.Core
{
    public interface ISearchService
    {
        Task<SearchResponseModel> GetResultFromSearch(string messageToSearch);
    }
}
