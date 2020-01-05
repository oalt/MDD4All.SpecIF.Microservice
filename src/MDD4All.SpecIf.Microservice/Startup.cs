/*
 * Copyright (c) MDD4All.de, Dr. Oliver Al
 * 
 * API versioning help:
 * https://dev.to/htissink/versioning-asp-net-core-apis-with-swashbuckle-making-space-potatoes-v-x-x-x-3po7
 */
using Consul;
using MDD4All.SpecIf.Microservice.DocumentFilters;
using MDD4All.SpecIf.Microservice.OperationFilters;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;

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
			services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                });

            services.AddSwaggerGen(options =>
            {
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

                //options.TagActionsBy()
            });




            string dataSource = Configuration.GetValue<string>("dataSource");

			string dataConnection = Configuration.GetValue<string>("dataConnection");

			if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
			{
				if (dataSource == "MongoDB")
				{
					services.AddScoped<ISpecIfMetadataReader>(dataProvider => new SpecIfMongoDbMetadataReader(dataConnection));
					services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfMongoDbDataReader(dataConnection));
					services.AddScoped<ISpecIfDataWriter>(dataProvider => new SpecIfMongoDbDataWriter(dataConnection, new SpecIfMongoDbMetadataReader(dataConnection)));

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
			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
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
