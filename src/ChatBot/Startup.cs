
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using BotServiceCollectionExtensions = ChatBot.Extensions.ServiceCollectionExtensions;
using ChatBot.Domain.Core;
using ChatBot.Domain.Services.IA;
using ChatBot.Domain.Services;
using ChatBot.Domain.Services.OpenData;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder.BotFramework;
using ChatBot.Domain.Core.Errors;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using ChatBot.Domain.Dialogs;
using ChatBot.Domain.Bot;
using ChatBot.Domain.Services.OpenData.Conf;
using ChatBot.Extensions;
using Microsoft.Bot.Configuration;

namespace ChatBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region BotConfigurationFile

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. ({botConfig})"));

            #endregion

            #region FrameworkServices

            services.AddControllersWithViews();
            services.AddRazorPages();


            #endregion

            #region Set Configuration

            services.AddConfiguration(BotServiceCollectionExtensions.ConfType.DegreeConfig, "opendata.json");
            
            #endregion

            #region Configure

            services.Configure<DegreeConfigModel>(services.GetConfiguration(BotServiceCollectionExtensions.ConfType.DegreeConfig));
           
            #endregion

            #region BotServices
            
            var connectedServices = new BotServices(botConfig);
            services.AddSingleton(sp => connectedServices);
            services.AddSingleton<ISpellCheckService, SpellCheckService>();
            services.AddSingleton<ISearchService, BingSearchService>();
            services.AddSingleton<ITeacherService, TeacherService>();
            services.AddSingleton<IUnexFacilitiesService, UnexFacilitiesService>();
            services.AddSingleton<IOpenDataService, OpenDataCacheService>();
            services.AddSingleton<OpenDataService>();
            #endregion

            #region Bot

            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainLuisDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, NexoBot<MainLuisDialog>>();

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
