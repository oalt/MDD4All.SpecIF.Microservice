/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Converters;
using MDD4All.SpecIF.DataModels.Helpers;
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
            Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID();
        }

		public Key(string id, string revision) 
		{
			ID = id;
			Revision = revision;
		}

		[JsonProperty(PropertyName = "id")]
		[BsonElement("id")]
		public string ID { get; set; } = "";

		[JsonProperty(PropertyName = "revision")]
		[BsonElement("revision")]
		public string Revision { get; set; } = SpecIfGuidGenerator.CreateNewSpecIfGUID();

		public override string ToString()
		{
			string result = "";

			result += ID + "_" + Revision;

			return result;
		}
	}
}
