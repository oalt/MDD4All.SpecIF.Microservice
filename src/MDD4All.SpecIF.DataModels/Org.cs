/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class Org : SpecIfElement
	{
		[JsonProperty(PropertyName = "organizationName")]
		[BsonElement("organizationName")]
		public string OrganizationName { get; set; }
	}
}
