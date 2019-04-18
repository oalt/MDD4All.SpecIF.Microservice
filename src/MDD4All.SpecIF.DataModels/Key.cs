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
		public static Revision LATEST_REVISION = new Revision()
		{
			BranchName = "main",
			RevsionNumber = 0
		};

		public static Revision FIRST_MAIN_REVISION = new Revision()
		{
			BranchName = "main",
			RevsionNumber = 1
		};

		public Key()
		{
		}

		public Key(string id)
		{
			ID = id;
			Revision = LATEST_REVISION;
		}

		public Key(string id, Revision revision) 
		{
			ID = id;
			Revision = revision;
		}

		public Key(string id, int revision)
		{
			ID = id;
			Revision = new Revision();

			Revision.RevsionNumber = revision;
		}

		[JsonProperty(PropertyName = "id")]
		[BsonElement("id")]
		public string ID { get; set; } = "";

		[JsonProperty(PropertyName = "revision")]
		[BsonElement("revision")]
		public Revision Revision { get; set; } = new Revision();
	}
}
