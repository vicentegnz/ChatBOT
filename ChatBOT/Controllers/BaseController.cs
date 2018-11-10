using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBOT.Controllers
{
    public class BotControllerBase : ControllerBase
    {
        protected readonly ILogger<BotControllerBase> _log;

        public BotControllerBase(IServiceProvider serviceProvider)
        {
            _log = serviceProvider.GetService<ILogger<BotControllerBase>>();
        }
    }
}