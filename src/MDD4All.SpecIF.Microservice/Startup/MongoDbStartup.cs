/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.Microservice.RightsManagement;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;
using MDD4All.SpecIF.DataProvider.MongoDB;
using MDD4All.SpecIF.DataProvider.MongoDB.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

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

                // SpecIF MongoDB connections
                services.AddScoped<ISpecIfMetadataReader>(dataProvider => new SpecIfMongoDbMetadataReader(dataConnection));
                services.AddScoped<ISpecIfMetadataWriter>(dataProvider => new SpecIfMongoDbMetadataWriter(dataConnection));

                services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfMongoDbDataReader(dataConnection));

                //services.AddScoped<ISpecIfDataReader>(dataProvider => new SpecIfJiraDataReader("https://mdd4all.atlassian.net", "Basic b2xpdmVyLmFsdEBtZGQ0YWxsLmRlOnF6eVo0cGg5d1JmRWFWRFFVdTdwREY4OQ=="));


                services.AddScoped<ISpecIfDataWriter>(dataProvider => new SpecIfMongoDbDataWriter(dataConnection, new SpecIfMongoDbMetadataReader(dataConnection),
                    new SpecIfMongoDbDataReader(dataConnection)));

            }
        }

    
        
    }
}
