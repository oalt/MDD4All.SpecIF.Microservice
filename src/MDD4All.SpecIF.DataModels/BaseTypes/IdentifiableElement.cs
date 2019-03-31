/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataModels.Service;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace MDD4All.SpecIF.DataModels.BaseTypes
{
    public class IdentifiableElement : SpecIfElement
    {
		public IdentifiableElement()
		{
			ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
			Revision = 1;
		}

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

		[JsonProperty(PropertyName = "id", Order = -100)]
		[BsonElement("id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "revision", Order = -99)]
		[BsonElement("revision")]
		public int Revision { get; set; }

		
	}
}
