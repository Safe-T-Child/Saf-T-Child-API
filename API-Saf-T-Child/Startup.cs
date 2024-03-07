using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;

namespace API_Saf_T_Child
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure MongoDBSettings and bind it to the MongoDBSettings class
            services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));

            // Configure services here
            services.AddMvc().AddControllersAsServices(); // Add MVC services
            services.AddAuthorization(); // Add authorization services

            // Inject IOptions<MongoDBSettings> into MongoDBService constructor
            services.AddSingleton<MongoDBService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            app.UseCors("AllowAll");
            
            app.UseRouting();

            // Add authorization middleware
            app.UseAuthorization();

            // Add endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Map controllers
            });
        }
    }
}

