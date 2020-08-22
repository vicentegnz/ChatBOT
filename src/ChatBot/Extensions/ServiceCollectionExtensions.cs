using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;

namespace ChatBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public enum ConfType
        {
            DegreeConfig
        }

        private static Dictionary<ConfType, string> ConfigPaths = new Dictionary<ConfType, string>();

        public static void AddConfiguration(this IServiceCollection services, ConfType confType, string configPath)
        {
            ConfigPaths.Add(confType, configPath);
        }

        public static IConfigurationRoot GetConfiguration(this IServiceCollection services, ConfType confType)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigPaths[confType]);

            return builder.Build();
        }

    }
}
