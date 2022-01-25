using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels.RightsManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDD4All.SpecIf.Microservice.Startup.MongoDB
{
    /// <summary>
    /// Check if the 
    /// </summary>
    public class AdminDbInitializer
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        private MongoDBDataAccessor<ApplicationRole> _roleMongoDbAccessor;
        private MongoDBDataAccessor<ApplicationUser> _userMongoDbAccessor;
        private MongoDBDataAccessor<JwtConfiguration> _jwtMongoDbAccessor;

        public AdminDbInitializer(IConfiguration configuration,
                                  ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void InitalizeAdminData()
        {
            string dataConnection = _configuration.GetValue<string>("dataConnection");

            if (!AdminDbExists(dataConnection))
            {
                _logger.LogInformation("Initialize specifAdmin data base...");
                CreateAdminUserAndRoles(dataConnection);
                CreateJwtConfiguration(dataConnection);
                _logger.LogInformation("Done.");
            }
            else
            {
                _logger.LogInformation("Existing specifAdmin data collections found. Nothing to initialize here.");
            }
        }

        private bool AdminDbExists(string connectionString)
        {
            bool result = true;

            

            MongoClient mongoClient = new MongoClient(connectionString);

            List<string> databaseNames = mongoClient.ListDatabaseNames().ToList();

            if(!databaseNames.Contains("specifAdmin"))
            {
                result = false;
            }

            return result;
        }

        public void CreateAdminUserAndRoles(string connectionString)
        {
            
            _roleMongoDbAccessor = new MongoDBDataAccessor<ApplicationRole>(connectionString, "specifAdmin");
            _userMongoDbAccessor = new MongoDBDataAccessor<ApplicationUser>(connectionString, "specifAdmin");

            _roleMongoDbAccessor.Add(new ApplicationRole
            {
                Id = "ROLE-Admin",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            });

            _roleMongoDbAccessor.Add(new ApplicationRole
            {
                Id = "ROLE-Editor",
                Name = "Editor",
                NormalizedName = "EDITOR"
            });

            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();

            ApplicationUser adminUser = new ApplicationUser
            {
                Id = "User-Admin",
                NormalizedUserName = "ADMIN",
                UserName = "admin",
                ApiKey = "F109BE57-BB60-42B5-A6E8-30DBDA7BEE2F",
                Roles = new List<string>
                {
                    "Administrator"
                }
            };

            string hash = passwordHasher.HashPassword(adminUser, "password");

            adminUser.PasswordHash = hash;

            _userMongoDbAccessor.Add(adminUser);

            ApplicationUser olliUser = new ApplicationUser
            {
                Id = "User-Olli",
                NormalizedUserName = "OLLI",
                UserName = "olli",
                ApiKey = "5824E518-C2C6-455A-9447-8A8CA029E40C",
                Roles = new List<string>
                {
                    "Administrator",
                    "Editor"
                }
            };

            string hashOlli = passwordHasher.HashPassword(adminUser, "password");

            olliUser.PasswordHash = hashOlli;

            _userMongoDbAccessor.Add(olliUser);
        }

        public void CreateJwtConfiguration(string connectionString)
        {
            _jwtMongoDbAccessor = new MongoDBDataAccessor<JwtConfiguration>(connectionString, "specifAdmin");

            JwtConfiguration jwtConfiguration = new JwtConfiguration()
            {
                Secret = Guid.NewGuid().ToString()
            };

            _jwtMongoDbAccessor.Add(jwtConfiguration);
        }
    }
}
