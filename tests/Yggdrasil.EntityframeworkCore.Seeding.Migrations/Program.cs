using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseDefaultServiceProvider(s => s.ValidateScopes = false)
                .UseStartup<Startup>();
    }
}
