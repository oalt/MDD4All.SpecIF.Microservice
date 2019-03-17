/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	public class Creator : SpecIfElement
	{
		[JsonProperty(PropertyName = "familyName")]
		[BsonElement("familyName")]
		public string FamilyName { get; set; }

		[JsonProperty(PropertyName = "givenName")]
		[BsonElement("givenName")]
		public string GivenName { get; set; }

		[JsonProperty(PropertyName = "org")]
		[BsonElement("org")]
		public Org Organization { get; set; }

		[JsonProperty(PropertyName = "email")]
		[BsonElement("email")]
		public Email Email { get; set; }
	}
}
