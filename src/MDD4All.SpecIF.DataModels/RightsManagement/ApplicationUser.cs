using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels.RightsManagement
{
	public class ApplicationUser
	{
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[BsonElement("userName")]
		public string UserName { get; set; }

		[BsonElement("normalizedUserName")]
		public string NormalizedUserName { get; set; }

		[BsonElement("passwordHash")]
		public string PasswordHash { get; set; }

		[BsonElement("roles")]
		public List<string> Roles { get; set; } = new List<string>();
	}
}
