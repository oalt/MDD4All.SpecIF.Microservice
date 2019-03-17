/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class HierarchyClass : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "icon")]
		[BsonElement("icon")]
		public string Icon { get; set; }

		[JsonProperty(PropertyName = "instantiation")]
		[BsonElement("instantiation")]
		public string Instantiation { get; set; }

		[JsonProperty(PropertyName = "propertyClasses")]
		[BsonElement("propertyClasses")]
		public List<Key> PropertyClasses { get; set; }
	}
}
