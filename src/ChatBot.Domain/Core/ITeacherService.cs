using ChatBot.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBot.Domain.Core
{
    public interface ITeacherService
    {
        Task<List<TeacherModel>> GetListOfTeachers();
    }
}
