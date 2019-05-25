using ChatBOT.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBOT.Core
{
    public interface ITeacherService
    {
        Task<List<TeacherModel>> GetListOfTeachers();
    }
}
