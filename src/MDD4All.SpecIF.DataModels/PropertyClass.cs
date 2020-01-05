/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class PropertyClass : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "dataType")]
		[BsonElement("dataType")]
		public Key DataType { get; set; }

		[JsonProperty(PropertyName = "multiple")]
		[BsonElement("multiple")]
		public bool? Multiple { get; set; }		
	}
}
