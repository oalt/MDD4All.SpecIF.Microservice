/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Converters;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	[JsonConverter(typeof(KeyConverter))]
	public class Key : SpecIfElement
	{
		public static string LATEST_REVISION = "main/latest";
		public static string FIRST_MAIN_REVISION = "main/1";

		public Key()
		{
		}

		public Key(string id)
		{
			ID = id;
			Revision = LATEST_REVISION;
		}

		public Key(string id, string revision) 
		{
			ID = id;
			Revision = revision;
		}

		public Key(string id, int revision)
		{
			ID = id;
			Revision = "main/"+ revision;
		}

		[JsonProperty(PropertyName = "id")]
		[BsonElement("id")]
		public string ID { get; set; } = "";

		[JsonProperty(PropertyName = "revision")]
		[BsonElement("revision")]
		public string Revision { get; set; }
	}
}
