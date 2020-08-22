
using ChatBot.Domain.Core;
using ChatBot.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ChatBot.Domain.Services.OpenData
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
