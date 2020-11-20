using Consul;
using MDD4All.SpecIf.Microservice.DocumentFilters;
using MDD4All.SpecIf.Microservice.Hubs;
using MDD4All.SpecIf.Microservice.OperationFilters;
using MDD4All.SpecIF.DataModels.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MDD4All.SpecIf.Microservice.Startup
{
    public abstract class StartupBase
    {
        protected readonly ILogger _logger;

        public IConfiguration Configuration { get; }

        private ISpecIfServiceDescription _serviceDescription;

        public static List<string> Urls = new List<string> { "https://localhost:888", "http://localhost:887" };

        public StartupBase(IConfiguration configuration, ILogger<StartupBase> logger)
        {
            Configuration = configuration;

            _logger = logger;

            _serviceDescription = ServiceDescriptionFactory.Create(ServiceStarter.Type, null);

            
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddMvc()
                .AddNewtonsoftJson(options => 
                { 
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; 
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });  

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;

                });

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;

                });

            services.AddRazorPages()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;

                });

            services.AddSwaggerGenNewtonsoftSupport();

            // CORS
            services.AddCors(o => o.AddPolicy("ActivateCorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // API versioning
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    //options.UseApiBehavior = false;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                });

            // Swagger generation
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllParametersInCamelCase();

                options.SwaggerDoc("v1.0", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "SpecIF API", // (" + _serviceDescription.ServiceName + ")",
                    Version = "v1.0",
                    Description = "Web API for the Specification Integration Facility (SpecIF).",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "The SpecIF team at Gesellschaft für Systems Engineering (GfSE) e.V.",
                        Url = new Uri("https://specif.de")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense
                    {
                        Name = "Apache License 2.0",
                        Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0")
                    }
                });

                options.OperationFilter<RemoveVersionFromParameter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                // Ensure the routes are added to the right Swagger doc
                options.DocInclusionPredicate((version, desc) =>
                {
                    var versions = desc.CustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    var maps = desc.CustomAttributes()
                        .OfType<MapToApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions)
                        .ToArray();

                    return versions.Any(v => $"v{v.ToString()}" == version)
                                  && (!maps.Any() || maps.Any(v => $"v{v.ToString()}" == version)); ;
                });

                options.EnableAnnotations();

                string filePath = Path.Combine(System.AppContext.BaseDirectory, "MDD4All.SpecIf.Microservice.xml");
                options.IncludeXmlComments(filePath);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });

            });


            ConfigureSpecIfDataServices(services);



            // Consul
            services.AddSingleton(_serviceDescription);

            

            
        }

        



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            app.UseHttpsRedirection();

            app.UseAuthentication();
            

            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");


            });

            app.UseCors("ActivateCorsPolicy");


            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                if (ServiceStarter.Type == "integration")
                {
                    endpoints.MapControllerRoute(
                        name: "services",
                        pattern: "services",
                        defaults: new { controller = "Service", action = "Index" });
                }
                else
                {
                    endpoints.MapControllerRoute(
                        name: "services",
                        pattern: "services",
                        defaults: new { controller = "Home", action = "Index" });
                }

                try
                {
                    endpoints.MapHub<SpecIfEventHub>("/specifEventHub");
                    
                }
                catch (Exception exception)
                {
                    _logger.LogWarning("Unable to start signalR.");
                }



            });

            // register with consul
            try
            {
                ConsulClient consulClient = new ConsulClient();

                lifetime.ApplicationStopping.Register(() =>
                {
                    consulClient.Agent.ServiceDeregister(_serviceDescription.ID).Wait();
                });
            }
            catch (Exception exception)
            {
                _logger.LogWarning("Unable to register in consul.");
            }

            

        }

        public abstract void ConfigureSpecIfDataServices(IServiceCollection services);
        
    }
}
