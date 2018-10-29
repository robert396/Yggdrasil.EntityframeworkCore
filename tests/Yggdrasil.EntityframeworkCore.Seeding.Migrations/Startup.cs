using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Contexts;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Migrations;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Seeders;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ValidSeeder>();
            services.AddDbContext<SeederDbContext>(opts => opts.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")).UseSeeding());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var dbContext = app.ApplicationServices.GetService<SeederDbContext>();
            dbContext.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
