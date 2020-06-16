/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels.BaseTypes
{
    public class IdentifiableElement : SpecIfElement
    {
		public IdentifiableElement()
		{
			ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
			Revision = SpecIfGuidGenerator.CreateNewRevsionGUID();
		}

        /// <summary>
        /// ID used in MongoDB
        /// </summary>
		[JsonIgnore]
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public string Id
		{
			get
			{
				return (ID + "_R_" + Revision);
			}
			set
			{
			}
		}

        /// <summary>
        /// ID used in JSON
        /// </summary>
		[JsonProperty(PropertyName = "id", Order = -101)]
		[BsonElement("id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "revision", Order = -92)]
		[BsonElement("revision")]
		public string Revision { get; set; }

		[JsonProperty(PropertyName = "replaces", Order = -91)]
		[BsonElement("replaces")]
		public List<string> Replaces { get; set; } = new List<string>();

	}
}
