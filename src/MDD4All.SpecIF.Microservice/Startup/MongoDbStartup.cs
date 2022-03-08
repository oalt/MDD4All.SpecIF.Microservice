/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MDD4All.SpecIF.Microservice.Startup.MongoDB;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class MongoDbStartup : StartupBase
	{
		
		public MongoDbStartup(IConfiguration configuration, ILogger<MongoDbStartup> logger) : 
            base(configuration, logger)
		{

		}

        public override void ConfigureSpecIfDataServices(IServiceCollection services)
        {
            string dataSource = Configuration.GetValue<string>("dataSource");

            string dataConnection = Configuration.GetValue<string>("dataConnection");

            if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
            {

                AdminDbInitializer adminDbInitializer = new AdminDbInitializer(Configuration, _logger);

                adminDbInitializer.InitalizeAdminData();

                // SpecIF MongoDB connections
                services.AddScoped<ISpecIfMetadataReader>(dataProvider => new SpecIfMongoDbMetadataReader(dataConnection));
                services.AddScoped<ISpecIfMetadataWriter>(dataProvider => new SpecIfMongoDbMetadataWriter(dataConnection));

                services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfMongoDbDataReader(dataConnection));

                services.AddScoped<ISpecIfDataWriter>(dataProvider => new SpecIfMongoDbDataWriter(dataConnection, 
                                                                                                  new SpecIfMongoDbMetadataReader(dataConnection),
                                                                                                  new SpecIfMongoDbDataReader(dataConnection)));

            }
        }

    
        
    }
}
