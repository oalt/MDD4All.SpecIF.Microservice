/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class Statement : Resource
	{
		[JsonProperty(PropertyName = "subject")]
		[BsonElement("subject")]
		public Key StatementSubject { get; set; }

		[JsonProperty(PropertyName = "object")]
		[BsonElement("object")]
		public Key StatementObject { get; set; }
	}
}
