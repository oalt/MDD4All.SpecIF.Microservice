/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Service;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace MDD4All.SpecIF.DataModels.BaseTypes
{
    public class SpecIfBaseElement : IdentifiableElement
    {
		public SpecIfBaseElement()
		{
			ChangedAt = DateTime.Now;
		}

		[JsonProperty(PropertyName = "title")]
		[BsonElement("title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "description")]
		[BsonElement("description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "changedAt")]
		[BsonElement("changedAt")]
		public DateTime ChangedAt { get; set; }

		[JsonProperty(PropertyName = "changedBy")]
		[BsonElement("changedBy")]
		public string ChangedBy { get; set; }

		
	}
}
