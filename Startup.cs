
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
using Microsoft.Bot.Builder.Integration;
using Microsoft.Extensions.Options;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;

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

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. ({botConfig})"));

            // Initialize Bot Connected Services clients.
            var connectedServices = new BotServices(botConfig);
            services.AddSingleton(sp => connectedServices);

            services.AddSingleton<ISpellCheckService, SpellCheckService>();
            services.AddSingleton<ISearchService, BingSearchService>();
            services.AddSingleton<ITeacherService, TeacherService>();
            services.AddSingleton<IScheduleService, ScheduleService>();
            services.AddSingleton<ISubjectService, SubjectService>();
            
            services.AddBot<NexoBot>(Options =>
            {
                var conversationState = new ConversationState(new MemoryStorage());
                Options.State.Add(conversationState);
                Options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
            });

            services.AddSingleton(serviceProvider => {

                var options = serviceProvider.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();

                var accessors = new NexoBotAccessors(conversationState)
                {
                    DialogStateAccessor = conversationState.CreateProperty<DialogState>(NexoBotAccessors.DialogStateAccessorName),
                    NexoBotStateStateAccessor = conversationState.CreateProperty<NexoBotState>(NexoBotAccessors.NexoBotStateAccesorName),
                };

                return accessors;

            });
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAutoMapper();
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
