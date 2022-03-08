/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.EA;
using MDD4All.SpecIF.DataProvider.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EAAPI = EA;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class EaStartup : StartupBase
    {
        public EaStartup(IConfiguration configuration, ILogger<EaStartup> logger) : base(configuration, logger)
        {
        }

        public override void ConfigureSpecIfDataServices(IServiceCollection services)
        {
            string dataSource = Configuration.GetValue<string>("dataSource");

            string dataConnection = Configuration.GetValue<string>("dataConnection");

            string eaConnectionString = Configuration.GetValue<string>("EaConnectionString");

            if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
            {
                // SpecIF MongoDB connections
                services.AddScoped<ISpecIfMetadataReader>(dataProvider => new SpecIfMongoDbMetadataReader(dataConnection));
                services.AddScoped<ISpecIfMetadataWriter>(dataProvider => new SpecIfMongoDbMetadataWriter(dataConnection));

               
                services.AddScoped<ISpecIfDataWriter>(dataProvider => new SpecIfMongoDbDataWriter(dataConnection, new SpecIfMongoDbMetadataReader(dataConnection),
                    new SpecIfMongoDbDataReader(dataConnection)));

                try
                {
                    ISpecIfMetadataReader metadataReader = new SpecIfMongoDbMetadataReader(dataConnection);

                    string progId = "EA.Repository";
                    Type type = Type.GetTypeFromProgID(progId);
                    EAAPI.Repository repository = Activator.CreateInstance(type) as EAAPI.Repository;

                    _logger.LogInformation("Starting Enterprise Architect...");

                    bool openResult = repository.OpenFile(eaConnectionString);

                    if (openResult)
                    {

                        repository.ShowWindow(1);

                        _logger.LogInformation("Model open.");


                        // TODO: Fix this:
                        //EaDataIntegrator eaDataIntegrator = new EaDataIntegrator(repository, metadataReader);

                        

                        services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfEaDataReader(repository, metadataReader));

                    }

                }
                catch 
                {

                }
            }
        }
    }
}
