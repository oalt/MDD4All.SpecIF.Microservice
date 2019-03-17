/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class DataType : SpecIfBaseElement
	{		
		[JsonProperty(PropertyName = "type")]
		[BsonElement("type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "maxLength")]
		[BsonElement("maxLength")]
		public int? MaxLength { get; set; }

		[JsonProperty(PropertyName = "accuracy")]
		[BsonElement("accuracy")]
		public int? Accuracy { get; set; }

		[JsonProperty(PropertyName = "min")]
		[BsonElement("min")]
		public int? Min { get; set; }

		[JsonProperty(PropertyName = "max")]
		[BsonElement("max")]
		public int? Max { get; set; }

		[JsonProperty(PropertyName = "values")]
		[BsonElement("value")]
		public List<EnumValue> Values { get; set; }

		/// <summary>
		/// Optional use by dataType 'xs:enumeration'. Indicates whether multiple values can be chosen; 
		/// by default the value is 'false'.
		/// </summary>
		[JsonProperty(PropertyName = "multiple")]
		[BsonElement("multiple")]
		public bool Multiple { get; set; }		

		

		
		
		
	}
}
