/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace MDD4All.SpecIF.DataModels
{
	public class File
	{
		[JsonIgnore]
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public string Id { get; set; }
	
		[JsonProperty(PropertyName = "id")]
		[BsonElement("id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "title")]
		[BsonElement("title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "type")]
		[BsonElement("type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "changedAt")]
		[BsonElement("changedAt")]
		public DateTime ChangedAt { get; set; }

		[JsonProperty(PropertyName = "changedBy")]
		[BsonElement("changedBy")]
		public string ChangedBy { get; set; }
	}
}
