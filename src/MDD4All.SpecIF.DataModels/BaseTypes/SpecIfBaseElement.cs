/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Converters;
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

		[JsonProperty(PropertyName = "title", Order = -97)]
		[BsonElement("title")]
        public object Title { get; set; } 

		[JsonProperty(PropertyName = "description", Order = -96)]
		[BsonElement("description")]
        //[JsonConverter(typeof(ValueConverter))]
        public object Description { get; set; } 

		[JsonProperty(PropertyName = "changedAt")]
		[BsonElement("changedAt")]
		public DateTime ChangedAt { get; set; }

		[JsonProperty(PropertyName = "changedBy")]
		[BsonElement("changedBy")]
		public string ChangedBy { get; set; }

		
	}
}
