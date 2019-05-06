using ChatBOT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Core
{
    public interface ISubjectService
    {
        List<StudyCenterModel> GetStudyCenters();
        List<DegreeModel> GetDegrees();
        List<SubjectModel> GetSubjectbyDegreeCode();
    }
}
