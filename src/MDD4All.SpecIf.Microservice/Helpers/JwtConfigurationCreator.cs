using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts.DataModels;
using System;

namespace MDD4All.SpecIf.Microservice.Helpers
{
    public class JwtConfigurationCreator
    {
        public JwtConfigurationCreator()
        {
            MongoDBDataAccessor<JwtConfiguration> jwtMongoDbAccessor = new MongoDBDataAccessor<JwtConfiguration>("mongodb://localhost:27017", "specifAdmin");

            JwtConfiguration jwtConfiguration = new JwtConfiguration()
            {
                Secret = Guid.NewGuid().ToString()
            };

            jwtMongoDbAccessor.Add(jwtConfiguration);
        }
    }
}
