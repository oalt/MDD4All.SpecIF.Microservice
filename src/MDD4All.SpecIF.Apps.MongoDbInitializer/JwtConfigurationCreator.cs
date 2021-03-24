using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels.RightsManagement;
using System;

namespace MDD4All.SpecIF.Apps.MongoDbInitializer
{
    public class JwtConfigurationCreator
    {
        public JwtConfigurationCreator()
        {
            
        }

        public void CreateJwtConfiguration(string connectionString)
        {
            MongoDBDataAccessor<JwtConfiguration> jwtMongoDbAccessor = new MongoDBDataAccessor<JwtConfiguration>(connectionString, "specifAdmin");

            JwtConfiguration jwtConfiguration = new JwtConfiguration()
            {
                Secret = Guid.NewGuid().ToString()
            };

            jwtMongoDbAccessor.Add(jwtConfiguration);
        }
    }
}
