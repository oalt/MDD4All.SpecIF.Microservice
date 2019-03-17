/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataProvider.Contracts.DataModels
{
    public class SpecIfIdentifier
    {

		public SpecIfIdentifier()
		{
			Id = System.Guid.NewGuid().ToString();
		}

		[JsonIgnore]
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public string Id
		{
			get; set;
		}

		[BsonElement("prefix")]
		public string Prefix { get; set; }

		[BsonElement("number")]
		public long Number { get; set; }
    }
}
