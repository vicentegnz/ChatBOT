using ChatBOT.Core;
using ChatBOT.Domain;
using ChatBOT.Services.OpenData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBOT.Services
{
    public class OpenDataCacheService : IOpenDataService
    {
        private readonly IOpenDataService _openDataService;

        public OpenDataCacheService(IServiceProvider serviceProvider)
        {
            _openDataService = serviceProvider.GetService<OpenDataService>();
        }

        public List<StudyCenterModel> GetStudyCenters()
        {
            var centers = OpenDataInfoCache.GetCentersModel();

            if (centers == null)
            {
                centers = _openDataService.GetStudyCenters();
                OpenDataInfoCache.SetCentersModel(centers);
            }

            return centers;
        }
    }

}
