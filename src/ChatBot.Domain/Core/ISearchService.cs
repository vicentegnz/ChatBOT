
using ChatBot.Entities;
using System.Threading.Tasks;

namespace ChatBot.Domain.Core
{
    public interface ISearchService
    {
        Task<SearchResponseModel> GetResultFromSearch(string messageToSearch);
    }
}
