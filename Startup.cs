using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniversityOfExtremaduraBOT.Bot;

namespace UniversityOfExtremaduraBOT
{
    public class Startup
    {

        public string ContentRootPath { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            ContentRootPath = env.ContentRootPath;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder().SetBasePath(ContentRootPath).AddJsonFile("appsettings.json").AddEnvironmentVariables();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var configuration = builder.Build();
            services.AddBot<SimpleBot>(Options =>
            {
                Options.CredentialProvider = new ConfigurationCredentialProvider(configuration);
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseBotFramework();
        }
    }
}
