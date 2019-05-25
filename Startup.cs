﻿
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChatBOT.Bot;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using AutoMapper;
using System;
using Microsoft.Bot.Configuration;
using ChatBot.Services;
using ChatBOT.Core;
using ChatBOT.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Extensions.Options;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using BotServiceCollectionExtensions = ChatBOT.Core.Extensions.ServiceCollectionExtensions;

using ChatBOT.Core.Extensions;
using ChatBOT.Domain;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using ChatBOT.Core.Errors;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Connector.Authentication;
using ChatBOT.Dialogs;

namespace ChatBOT
{
    public class Startup
    {
        public string ContentRootPath { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            ContentRootPath = env.ContentRootPath;

            var builder = new ConfigurationBuilder().SetBasePath(ContentRootPath).AddJsonFile("appsettings.json").AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //Framework services
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAutoMapper();

            #region "Set Configuration"
            services.AddConfiguration(BotServiceCollectionExtensions.ConfType.DegreeConfig, "Conf\\opendata.json");
            #endregion

            #region "Configure"
            services.Configure<DegreeConfigModel>(services.GetConfiguration(BotServiceCollectionExtensions.ConfType.DegreeConfig));
            #endregion

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. ({botConfig})"));

            // Initialize Bot Connected Services clients.
            var connectedServices = new BotServices(botConfig);
            services.AddSingleton(sp => connectedServices);


            services.AddSingleton<ISpellCheckService, SpellCheckService>();
            services.AddSingleton<ISearchService, BingSearchService>();
            services.AddSingleton<ITeacherService, TeacherService>();
            services.AddSingleton<OpenDataService>();
            services.AddSingleton<IOpenDataService, OpenDataCacheService>();



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
           
            var resourceExplorer = ResourceExplorer.LoadProject(Directory.GetCurrentDirectory(), ignoreFolders: new string[] { "models" });
            services.AddSingleton(resourceExplorer);

        }




        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            env.ConfigureNLog("Conf/nlog.config");
            loggerFactory.AddNLog();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseMvc();
            ;
        }
    }
}
