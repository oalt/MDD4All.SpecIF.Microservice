using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels.RightsManagement;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.Apps.MongoDbInitializer
{
    class AdminUserCreator
    {
        private MongoDBDataAccessor<ApplicationRole> _roleMongoDbAccessor;
        private MongoDBDataAccessor<ApplicationUser> _userMongoDbAccessor;

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
    }

    
}
