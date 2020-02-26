/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	/// <summary>
	/// Enumerated Value
	/// </summary>
	public class EnumValue : SpecIfElement
	{
		[JsonProperty(PropertyName = "id")]
		[BsonElement("id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "title")]
		[BsonElement("title")]
		public object Title { get; set; }
	}
}
