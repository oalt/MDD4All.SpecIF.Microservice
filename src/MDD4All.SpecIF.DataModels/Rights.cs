/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class Rights : SpecIfElement
	{
		[JsonProperty(PropertyName = "title")]
		[BsonElement("title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "type")]
		[BsonElement("type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "url")]
		[BsonElement("url")]
		public string URL { get; set; }
	}
}
