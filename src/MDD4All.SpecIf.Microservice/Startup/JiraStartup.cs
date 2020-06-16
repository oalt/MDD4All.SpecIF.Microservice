using System.Text;
using MDD4All.SpecIf.Microservice.RightsManagement;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;
using MDD4All.SpecIF.DataProvider.Jira;
using MDD4All.SpecIF.DataProvider.MongoDB;
using MDD4All.SpecIF.DataProvider.MongoDB.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MDD4All.SpecIf.Microservice.Startup
{
    public class JiraStartup : StartupBase
    {
        public JiraStartup(IConfiguration configuration) : base(configuration)
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

                // SpecIF MongoDB connections
                services.AddScoped<ISpecIfMetadataReader>(dataProvider => new SpecIfMongoDbMetadataReader(dataConnection));
                services.AddScoped<ISpecIfMetadataWriter>(dataProvider => new SpecIfMongoDbMetadataWriter(dataConnection));

                ISpecIfDataReader specIfDataReader = new SpecIfJiraDataReader(jiraServer, 
                                                                              jiraAuth,
                                                                              new SpecIfMongoDbMetadataReader(dataConnection));

                services.AddScoped<ISpecIfDataReader>(dataProvider => specIfDataReader);


                services.AddScoped<ISpecIfDataWriter>(dataProvider => new SpecIfJiraDataWriter(jiraServer, 
                                                                                               jiraAuth, 
                                                                                               new SpecIfMongoDbMetadataReader(dataConnection),
                                                                                               specIfDataReader));

            }
        }

  
    }
}
