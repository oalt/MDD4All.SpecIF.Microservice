/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	/// <summary>
	/// The resources such as diagrams, model elements or requirements.
	/// </summary>
	public class Resource : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "class")]
		[BsonElement("class")]
		public Key ResourceClass { get; set; }

		[JsonProperty(PropertyName = "properties")]
		[BsonElement("properties")]
		public List<Property> Properties { get; set; }
	}
}
