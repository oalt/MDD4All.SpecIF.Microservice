using MDD4All.SpecIf.Microservice.RightsManagement;
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
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MDD4All.SpecIf.Microservice.Startup
{
    public class IntegrationStartup : StartupBase
    {
        public IntegrationStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureSpecIfDataServices(IServiceCollection services)
        {
            string dataSource = Configuration.GetValue<string>("dataSource");

            string dataConnection = Configuration.GetValue<string>("dataConnection");

            if (!string.IsNullOrEmpty(dataSource) && !string.IsNullOrEmpty(dataConnection))
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

                // Integration data provider

                SpecIfServiceDataProvider specIfServiceDataProvider = new SpecIfServiceDataProvider();
                services.AddSingleton<ISpecIfServiceDescriptionProvider>(specIfServiceDataProvider);

                SpecIfIntegrationMetadataReader metadataReader = new SpecIfIntegrationMetadataReader(specIfServiceDataProvider);
                services.AddSingleton<ISpecIfMetadataReader>(metadataReader);

                SpecIfIntegrationDataReader dataReader = new SpecIfIntegrationDataReader(specIfServiceDataProvider);
                services.AddSingleton<ISpecIfDataReader>(dataReader);

                SpecIfIntegrationDataWriter dataWriter = new SpecIfIntegrationDataWriter(specIfServiceDataProvider, metadataReader, dataReader);
                services.AddSingleton<ISpecIfDataWriter>(dataWriter);


                // cache
                SpecIfMongoDbDataWriter cacheDataWriter = new SpecIfMongoDbDataWriter(dataConnection, 
                    new SpecIfMongoDbMetadataReader(dataConnection),
                    new SpecIfMongoDbDataReader(dataConnection), "specifCache");

                SpecIfMongoDbDataReader cacheDataReader = new SpecIfMongoDbDataReader(dataConnection, "specifCache");

                dataReader.ActivateCache(cacheDataWriter, cacheDataReader);



            }
        }
    }
}
