using ChatBot.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBot.Domain.Core
{
    public interface IUnexFacilitiesService
    {
        Task<List<UnexFacilitieModel>> GetUnexFacilities();

        Task<List<string>> GetUnexFacilitiesCategories();

    }
}
