using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels.RightsManagement
{
    public class JwtConfiguration
    {
        public const string ID = "0D43EFC6-9D2E-4D41-A864-83E5E6CB7C3A";

        public JwtConfiguration()
        {
            Id = ID;
        }

        [JsonIgnore]
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id
        {
            get; set;
        }

        [BsonElement("jwtSecret")]
        public string Secret { get; set; }

        [BsonElement("issuer")]
        public string Issuer { get; set; } = "specificator.com";
    }
}
