/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class Property : SpecIfBaseElement
	{
		public Property()
		{
			Value = new Value();
		}

		[JsonProperty(PropertyName = "class")]
		[BsonElement("class")]
		public Key PropertyClass { get; set; }

		[JsonProperty(PropertyName = "value")]
		[BsonElement("value")]
		public Value Value
		{
			get; set;
		}
			
	}
}
