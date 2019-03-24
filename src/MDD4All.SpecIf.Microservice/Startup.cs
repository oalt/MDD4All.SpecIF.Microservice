/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Consul;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

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
			services.AddMvc();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "SpecIF API", Version = "v1" });
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

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpecIF API V1");
			});

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			// register with consul
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

			lifetime.ApplicationStopping.Register(() => {
				
				consulClient.Agent.ServiceDeregister(registration.ID).Wait();
			});
		}

		

	}
}
