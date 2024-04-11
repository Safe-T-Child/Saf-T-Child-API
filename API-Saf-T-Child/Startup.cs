using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

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
            // Add JWT Authentication Services here as shown in Program.cs
            var jwtIssuer = Configuration.GetSection("Jwt:Issuer").Get<string>();
            var jwtKey = Configuration.GetSection("Jwt:Key").Get<string>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var exceptionType = context.Exception.GetType();
                        if (exceptionType == typeof(SecurityTokenInvalidIssuerException))
                        {
                            context.Response.Headers.Add("Token-Error-Issuer", "Invalid issuer");
                        }
                        else if (exceptionType == typeof(SecurityTokenInvalidAudienceException))
                        {
                            context.Response.Headers.Add("Token-Error-Audience", "Invalid audience");
                        }
                        else if (exceptionType == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Error-Expired", "Token expired");
                        }
                        else if (exceptionType == typeof(SecurityTokenNotYetValidException))
                        {
                            context.Response.Headers.Add("Token-Error-NotYetValid", "Token not yet valid");
                        }
                        context.Response.Headers.Add("Token-Error-Detail", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Saf-T-Child-Services", Version = "v1" });

                // Define the Bearer Authentication Scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            // Configure MongoDBSettings and bind it to the MongoDBSettings class
            services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));
            services.Configure<MailSettings>(Configuration.GetSection("Mail"));

            // Configure services here
            services.AddMvc().AddControllersAsServices(); // Add MVC services
            services.AddAuthorization(); // Add authorization services

            // Inject IOptions<MongoDBSettings> into MongoDBService constructor
            services.AddSingleton<MongoDBService>();
            services.AddSingleton<MessageService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            // Add http middleware to redirect to https
            app.UseMiddleware<SelectiveHttpsMiddleware>();
            
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Saf-T-Child-Services V1");
                    c.RoutePrefix = string.Empty;  // Set the Swagger UI at the app's root
                });
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
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Add endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Map controllers
            });
        }
    }
}

