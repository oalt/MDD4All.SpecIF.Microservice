/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.Microservice.RightsManagement;
using MDD4All.SpecIF.DataIntegrator.Contracts;
using MDD4All.SpecIF.DataIntegrator.KafkaListener;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;
using MDD4All.SpecIF.DataProvider.Integration;
using MDD4All.SpecIF.DataProvider.MongoDB;
using MDD4All.SpecIF.DataProvider.MongoDB.Authorization;
using MDD4All.SpecIF.ServiceDataProvider;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class IntegrationStartup : StartupBase
    {
        public IntegrationStartup(IConfiguration configuration, ILogger<IntegrationStartup> logger) : base(configuration, logger)
        {
        }

        public override void ConfigureSpecIfDataServices(IServiceCollection services)
        {
            string dataSource = Configuration.GetValue<string>("dataSource");

            string dataConnection = "";
            //dataConnection = Environment.GetEnvironmentVariable("dataConnection");
            //if (dataConnection == "" || dataConnection == null)
            //{
                 dataConnection = Configuration.GetValue<string>("dataConnection");
           // }
           

            if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
            {

                //    // user and role management
                //    services.AddScoped<IUserStore<ApplicationUser>>(userStore =>
                //    {
                //        return new SpecIfApiUserStore(dataConnection);

                //    });

                //    services.AddScoped<IUserRoleStore<ApplicationUser>>(userStore =>
                //    {
                //        return new SpecIfApiUserStore(dataConnection);

                //    });

                //    services.AddScoped<IRoleStore<ApplicationRole>>(roleStore =>
                //    {
                //        return new SpecIfApiRoleStore(dataConnection);
                //    });

                //IJwtConfigurationReader jwtConfigurationReader = new MongoDbJwtConfigurationReader(dataConnection);

                //services.AddSingleton<IJwtConfigurationReader>(jwtConfigurationReader);

                //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                //.AddJwtBearer(options =>
                //{
                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        ValidateIssuer = true,
                //        ValidateAudience = true,
                //        ValidateLifetime = true,
                //        ValidateIssuerSigningKey = true,
                //        ValidIssuer = jwtConfigurationReader.GetIssuer(),
                //        ValidAudience = jwtConfigurationReader.GetIssuer(),
                //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfigurationReader.GetSecret()))
                //    };
                //})
                //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => Configuration.Bind("CookieSettings", options));

                // Integration data provider

                SpecIfServiceDataProvider specIfServiceDataProvider = new SpecIfServiceDataProvider();
                services.AddSingleton<ISpecIfServiceDescriptionProvider>(specIfServiceDataProvider);

                SpecIfIntegrationMetadataReader metadataReader = new SpecIfIntegrationMetadataReader(specIfServiceDataProvider);
                services.AddSingleton<ISpecIfMetadataReader>(metadataReader);

                SpecIfIntegrationDataReader dataReader = new SpecIfIntegrationDataReader(specIfServiceDataProvider);
                services.AddSingleton<ISpecIfDataReader>(dataReader);

                string integrationApiKey = Configuration.GetValue<string>("IntegrationApiKey");

                SpecIfIntegrationDataWriter dataWriter = new SpecIfIntegrationDataWriter(integrationApiKey, specIfServiceDataProvider, metadataReader, dataReader);
                services.AddSingleton<ISpecIfDataWriter>(dataWriter);


                // cache
                SpecIfMongoDbDataWriter cacheDataWriter = new SpecIfMongoDbDataWriter(dataConnection, 
                    new SpecIfMongoDbMetadataReader(dataConnection),
                    new SpecIfMongoDbDataReader(dataConnection), "specifCache");

                SpecIfMongoDbDataReader cacheDataReader = new SpecIfMongoDbDataReader(dataConnection, "specifCache");

                dataReader.ActivateCache(cacheDataWriter, cacheDataReader);

                services.AddSignalR();

                KafkaSpecIfEventService kafkaSpecIfEventService = new KafkaSpecIfEventService("127.0.0.1:9092", "event-listener-group");

                services.AddSingleton<ISpecIfEventService>(kafkaSpecIfEventService);

            }
        }
    }
}
