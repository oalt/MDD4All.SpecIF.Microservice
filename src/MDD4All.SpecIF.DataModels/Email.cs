/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class Email : SpecIfElement
	{
		[JsonProperty(PropertyName = "type")]
		[BsonElement("type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "value")]
		[BsonElement("value")]
		public string Value { get; set; }
	}
}
