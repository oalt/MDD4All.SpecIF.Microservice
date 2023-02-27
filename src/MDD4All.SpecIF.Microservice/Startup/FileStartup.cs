/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataProvider.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class FileStartup : StartupBase
    {
        public FileStartup(IConfiguration configuration, ILogger<FileStartup> logger) :
            base(configuration, logger)
        {

        }

        public override void ConfigureSpecIfDataServices(IServiceCollection services)
        {
            string dataSource = Configuration.GetValue<string>("dataSource");

            string dataConnection = Configuration.GetValue<string>("dataConnection");

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

                // SpecIF File connections

                string fileMetadataRootPath = Configuration.GetValue<string>("FileMetadataRootPath");

                string fileDataRootPath = Configuration.GetValue<string>("FileDataRootPath");

                ISpecIfMetadataReader metadataReader = new SpecIfFileMetadataReader(fileMetadataRootPath);

                ISpecIfMetadataWriter metadataWriter = new SpecIfFileMetadataWriter(fileMetadataRootPath);
                
                ISpecIfDataReader dataReader = new SpecIfFileDataReader(fileDataRootPath);

                ISpecIfDataWriter dataWriter = new SpecIfFileDataWriter(fileDataRootPath, metadataReader, dataReader);

                services.AddSingleton<ISpecIfMetadataReader>(dataProvider => metadataReader);

                services.AddSingleton<ISpecIfMetadataWriter>(dataProvider => metadataWriter);

                services.AddSingleton<ISpecIfDataReader>(dataProvider => dataReader);

                services.AddScoped<ISpecIfDataWriter>(dataProvider => dataWriter);

            }
        }
    }
}
