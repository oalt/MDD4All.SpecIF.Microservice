using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;

namespace MDD4All.SpecIF.DataProvider.MongoDB.Authorization
{
    public class MongoDbJwtConfigurationReader : IJwtConfigurationReader
    {
        private const string SPECIF_ADMIN_DATABASE_NAME = "specifAdmin";

        private MongoDBDataAccessor<JwtConfiguration> _jwtMongoDbAccessor;

        private JwtConfiguration _jwtConfiguration;

        public MongoDbJwtConfigurationReader(string connectionString)
        {
            _jwtMongoDbAccessor = new MongoDBDataAccessor<JwtConfiguration>(connectionString, SPECIF_ADMIN_DATABASE_NAME);

            _jwtConfiguration = _jwtMongoDbAccessor.GetItemById(JwtConfiguration.ID);

        }

        public string GetIssuer()
        {
            return _jwtConfiguration.Issuer;
        }

        public string GetSecret()
        {
            return _jwtConfiguration.Secret;
        }
    }
}
