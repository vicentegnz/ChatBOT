
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
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
using Microsoft.Bot.Builder.Dialogs;
using ChatBOT.Dialog;

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
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;



            // Create conversation and user state management objects, using memory storage.
            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);

            // Create and register state accessors.
            // Accessors created here are passed into the IBot-derived class on every turn.
            services.AddSingleton<ComplexDialogBotAccessors>(sp =>
            {
                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new ComplexDialogBotAccessors(conversationState, userState)
                {
                    DialogStateAccessor = conversationState.CreateProperty<DialogState>("DialogState"),
                    QuestionStateAccesor = userState.CreateProperty<QuestionState>("QuestionState"),
                };

                return accessors;
            });
            
            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. ({botConfig})"));

            // Initialize Bot Connected Services clients.
            var connectedServices = new BotServices(botConfig);
            services.AddSingleton(sp => connectedServices);

            services.AddSingleton<ISpellCheckService, SpellCheckService>();
            services.AddSingleton<ISearchService, BingSearchService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAutoMapper();

            services.AddBot<NexoBot>(Options =>
            {
                Options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            env.ConfigureNLog("Conf/nlog.config");
            loggerFactory.AddNLog();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
