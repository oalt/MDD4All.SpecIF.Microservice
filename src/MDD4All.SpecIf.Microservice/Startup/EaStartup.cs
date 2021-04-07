using System;
using System.Text;
using MDD4All.SpecIf.Microservice.RightsManagement;
using MDD4All.SpecIF.DataIntegrator.EA;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;
using MDD4All.SpecIF.DataProvider.EA;
using MDD4All.SpecIF.DataProvider.Jira;
using MDD4All.SpecIF.DataProvider.MongoDB;
using MDD4All.SpecIF.DataProvider.MongoDB.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using EAAPI = EA;

namespace MDD4All.SpecIf.Microservice.Startup
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

            string jiraAuth = Configuration.GetValue<string>("JiraAuthorization");

            string jiraServer = Configuration.GetValue<string>("JiraServer");

            if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
            {

                

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

                    //bool openResult = repository.OpenFile(@"d:\alto_daten\EA\KafkaIntegrationTest.eapx");

                    // D:\work\github\SpecIF-Backend\src\MDD4All.SoecIF.DataProvider.EA.Test\TestData\TestModel1.eap

                    _logger.LogInformation("Starting Enterprise Architect...");

                    bool openResult = repository.OpenFile(@"D:\work\github\SpecIF-Backend\src\MDD4All.SoecIF.DataProvider.EA.Test\TestData\TestModel1.eap");

                    //bool openResult = repository.OpenFile(@"D:\alto_daten\EA\TestPackage.eapx");

                

                    if (openResult)
                    {

                        repository.ShowWindow(1);

                        _logger.LogInformation("Model open.");

                        EaDataIntegrator eaDataIntegrator = new EaDataIntegrator(repository, metadataReader);

                        

                        services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfEaDataReader(repository, metadataReader));

                    }

                }
                catch (Exception exception)
                {

                }
            }
        }
    }
}
