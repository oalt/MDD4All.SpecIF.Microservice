/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Consul;
using MDD4All.SpecIF.Microservice.DocumentFilters;
using MDD4All.SpecIF.Microservice.Hubs;
using MDD4All.SpecIF.Microservice.OperationFilters;
using MDD4All.SpecIF.Microservice.RightsManagement;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataModels.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace MDD4All.SpecIF.Microservice.Startup
{
    /// <summary>
    /// Common abstract base class to start the SpecIF Microservice.
    /// </summary>
    public abstract class StartupBase
    {
        protected readonly ILogger _logger;

        public IConfiguration Configuration { get; }

        private ISpecIfServiceDescription _serviceDescription;

        public static List<string> Urls = new List<string> { "https://127.0.0.1:888", "http://127.0.0.1:887" };

        public StartupBase(IConfiguration configuration, ILogger<StartupBase> logger)
        {
            Configuration = configuration;

            _logger = logger;

            _serviceDescription = ServiceDescriptionFactory.Create(ServiceStarter.Type, null);

            
        }



        ///
        /// This method gets called by the runtime. Use this method to add services to the container.
        ///
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
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole("Administrator"));
                options.AddPolicy("unregisteredReader", policy => policy.RequireRole("anonReader", "Reader", "Editor", "Administrator"));
                options.AddPolicy("registeredReader", policy => policy.RequireRole("Reader", "Editor", "Administrator"));
            });

            // API versioning
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    //options.UseApiBehavior = false;
                    options.DefaultApiVersion = new ApiVersion(1, 1);
                });

            // Swagger generation
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllParametersInCamelCase();

                options.SwaggerDoc("v1.1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "SpecIF API (" + _serviceDescription.ServiceName + ")",
                    Version = "v1.1",
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

                string filePath = Path.Combine(System.AppContext.BaseDirectory, "MDD4All.SpecIF.Microservice.xml");
                options.IncludeXmlComments(filePath);

                //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.ApiKey,
                //    Scheme = "Bearer",
                //    BearerFormat = "JWT",
                //    In = ParameterLocation.Header,
                //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                //});

                options.AddSecurityDefinition(ApiKeyConstants.HeaderName, new OpenApiSecurityScheme()
                {
                    Description = "API key needed to access the endpoints. X-API-KEY: My_API_Key",
                    In = ParameterLocation.Header,
                    Name = ApiKeyConstants.HeaderName,
                    Type = SecuritySchemeType.ApiKey

                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                    Name = ApiKeyConstants.HeaderName,
                                    Type = SecuritySchemeType.ApiKey,
                                    In = ParameterLocation.Header,
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = ApiKeyConstants.HeaderName
                                    }    
                            },
                            new string[] {}

                    }
                });

            });


            ConfigureSpecIfDataServices(services);

            ConfigureSecurityServices(services);

            // Consul
            services.AddSingleton(_serviceDescription);

            

            
        }





        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        [Obsolete]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
           // app.UseHttpsRedirection();

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
                c.SwaggerEndpoint("/swagger/v1.1/swagger.json", "v1.1");


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
                catch 
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
            catch 
            {
                _logger.LogWarning("Unable to register in consul.");
            }

            

        }

        /// <summary>
        /// Configure security.
        /// </summary>
        /// <param name="services"></param>
        protected void ConfigureSecurityServices(IServiceCollection services)
        {
            string dataSource = Configuration.GetValue<string>("dataSource");

            string dataConnection = Configuration.GetValue<string>("dataConnection");

            // user and role management
            services.AddScoped<IUserStore<ApplicationUser>>(userStore =>
            {
                return new SpecIfApiUserStore(dataConnection);

            });

            services.AddScoped<IUserRoleStore<ApplicationUser>>(userStore =>
            {
                return new SpecIfApiUserStore(dataConnection);

            });

            services.AddScoped<IRoleStore<ApplicationRole>>(roleStore =>
            {
                return new SpecIfApiRoleStore(dataConnection);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = SpecIfAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = SpecIfAuthenticationOptions.DefaultScheme;
            })
            .AddApiKeySupport(options => { });
        }

        /// <summary>
        /// Call this method to configure the data services (DataReader/Writer) for SpecIF data access.
        /// </summary>
        /// <param name="services"></param>
        public abstract void ConfigureSpecIfDataServices(IServiceCollection services);
        
    }
}
