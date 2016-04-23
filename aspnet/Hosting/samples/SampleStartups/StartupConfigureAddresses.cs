using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

// Note that this sample will not run. It is only here to illustrate usage patterns.

namespace SampleStartups
{
    public class StartupConfigureAddresses : StartupBase
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultHostingConfiguration(args)
                .UseStartup<StartupConfigureAddresses>()
                .UseUrls("http://localhost:5000", "http://localhost:5001")
                .Build();

            host.Run();
        }
    }
}
