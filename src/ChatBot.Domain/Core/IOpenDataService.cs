using ChatBot.Entities;
using System.Collections.Generic;

namespace ChatBot.Domain.Core
{
    public interface IOpenDataService
    {
        List<StudyCenterModel> GetStudyCenters();
    }
}
