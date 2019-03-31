/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class Statement : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "class", Order = -97)]
		[BsonElement("class")]
		public Key StatementClass { get; set; }

		[JsonProperty(PropertyName = "subject")]
		[BsonElement("subject")]
		public Key StatementSubject { get; set; }

		[JsonProperty(PropertyName = "object")]
		[BsonElement("object")]
		public Key StatementObject { get; set; }

		[JsonProperty(PropertyName = "properties")]
		[BsonElement("properties")]
		public List<Property> Properties { get; set; }

	}
}
