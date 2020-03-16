/*
 * Copyright (c) MDD4All.de, Dr. Oliver Al
 * 
 * API versioning help:
 * https://dev.to/htissink/versioning-asp-net-core-apis-with-swashbuckle-making-space-potatoes-v-x-x-x-3po7
 */
using Consul;
using MDD4All.SpecIf.Microservice.DocumentFilters;
using MDD4All.SpecIf.Microservice.OperationFilters;
using MDD4All.SpecIf.Microservice.RightsManagement;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;
using MDD4All.SpecIF.DataProvider.MongoDB;
using MDD4All.SpecIF.DataProvider.MongoDB.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDD4All.SpecIf.Microservice
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		private ISpecIfServiceDescription _serviceDescription;

		private string[] _tags;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}



		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
            string mongoDbConnectionString = "mongodb://localhost:27017";

            

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                
            });

            

            // CORS
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
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
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                });

            // Swagger generation
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllParametersInCamelCase();

                options.SwaggerDoc("v1.0", new Info
                            {
                                Title = "SpecIF API",
                                Version = "v1.0",
                                Description = "Web API for the Specification Integration Facility (SpecIF).",
                                Contact = new Contact
                                {
                                    Name = "The SpecIF team at Gesellschaft für Systems Engineering (GfSE) e.V.",
                                    Url = "https://specif.de"
                                },
                                License = new License
                                {
                                    Name = "Apache License 2.0",
                                    Url = "http://www.apache.org/licenses/LICENSE-2.0"
                                }
                            });

                options.OperationFilter<RemoveVersionFromParameter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                // Ensure the routes are added to the right Swagger doc
                options.DocInclusionPredicate((version, desc) =>
                {
                    var versions = desc.ControllerAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    var maps = desc.ActionAttributes()
                        .OfType<MapToApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions)
                        .ToArray();

                    return versions.Any(v => $"v{v.ToString()}" == version)
                                  && (!maps.Any() || maps.Any(v => $"v{v.ToString()}" == version)); ;
                });

                options.EnableAnnotations();

                string filePath = Path.Combine(System.AppContext.BaseDirectory, "MDD4All.SpecIf.Microservice.xml");
                options.IncludeXmlComments(filePath);

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                options.AddSecurityRequirement(security);

                //options.TagActionsBy()
            });




            string dataSource = Configuration.GetValue<string>("dataSource");

			string dataConnection = Configuration.GetValue<string>("dataConnection");

			if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
			{
				if (dataSource == "MongoDB")
				{
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

                    IJwtConfigurationReader jwtConfigurationReader = new MongoDbJwtConfigurationReader(dataConnection);

                    services.AddSingleton<IJwtConfigurationReader>(jwtConfigurationReader);

                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtConfigurationReader.GetIssuer(),
                            ValidAudience = jwtConfigurationReader.GetIssuer(),
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfigurationReader.GetSecret()))
                        };
                    })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => Configuration.Bind("CookieSettings", options)); ;

                    //services.AddIdentity<ApplicationUser, ApplicationRole>();

                    // SpecIF MongoDB connections
                    services.AddScoped<ISpecIfMetadataReader>(dataProvider => new SpecIfMongoDbMetadataReader(dataConnection));
					services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfMongoDbDataReader(dataConnection));
					services.AddScoped<ISpecIfDataWriter>(dataProvider => new SpecIfMongoDbDataWriter(dataConnection, new SpecIfMongoDbMetadataReader(dataConnection),
                        new SpecIfMongoDbDataReader(dataConnection)));

                    // Consul
					string serviceAddress = "http://localhost";
					int port = 888;

					_serviceDescription = new SpecIfServiceDescription()
					{
						ID = "{67FE892C-7EB1-45AD-9259-6BE910841A3A}",
						DataRead = true,
						DataWrite = true,
						MetadataRead = true,
						MetadataWrite = false,
						ServiceName = "SpecIF-MongoDB-Service",
						ServiceDescription = "This service provides SpecIF data from a MongoDB backend.",
						ServiceAddress = serviceAddress,
						ServicePort = port,
						IconURL = serviceAddress + ":" + port + "/images/MongoDBLogo.jpg"
					};

					_tags = new[] { "SpecIF-Service", "MongoDB" };

					services.AddSingleton(_serviceDescription);
				}
			}

		}

	

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
		{
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

            app.UseCors("MyPolicy");


            app.UseStaticFiles();

			app.UseMvc(routes =>
			{
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // register with consul
            try
            {
                ConsulClient consulClient = new ConsulClient();

                AgentServiceRegistration registration = new AgentServiceRegistration()
                {
                    ID = _serviceDescription.ID,
                    Name = _serviceDescription.ServiceName,
                    Address = _serviceDescription.ServiceAddress,
                    Port = _serviceDescription.ServicePort,
                    Tags = _tags
                };

                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                consulClient.Agent.ServiceRegister(registration).Wait();

                lifetime.ApplicationStopping.Register(() =>
                {

                    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                });
            }
            catch(Exception exception)
            {
                Console.WriteLine("Unable to register in consul.");
            }
		}

		

	}
}
