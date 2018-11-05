
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace UniversityOfExtremaduraBOT
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var host = new WebHostBuilder()
                .UseKestrel(options =>
            {
                options.Listen(IPAddress.Loopback,44318, listenOptions =>
                               listenOptions.UseHttps("localhost.pfx","1234"));
            })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("https://*:44318")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

    }
}
