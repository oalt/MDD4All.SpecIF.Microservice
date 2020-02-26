/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MDD4All.SpecIF.DataModels
{
    /// <summary>
    /// The resources such as diagrams, model elements or requirements.
    /// </summary>
    public class Resource : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "class", Order = -95)]
		[BsonElement("class")]
		public Key Class { get; set; }

        [JsonProperty(PropertyName = "language", Order = -94)]
        [BsonElement("language")]
        [DataMember(Name = "language")]
        public string Language { get; set; } = "en";

		[JsonProperty(PropertyName = "properties", Order = -93)]
		[BsonElement("properties")]
		public List<Property> Properties { get; set; }

        [JsonProperty(PropertyName = "alternativeIds", Order = -90)]
        [BsonElement("alternativeIds")]
        public List<AlternativeId> AlternativeIDs { get; set; } = new List<AlternativeId>();
	}
}
