/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class StatementClass : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "extends")]
		[BsonElement("extends")]
		public Key Extends { get; set; }

		[JsonProperty(PropertyName = "icon")]
		[BsonElement("icon")]
		public string Icon { get; set; }

		[JsonProperty(PropertyName = "instantiation")]
		[BsonElement("instantiation")]
		public List<string> Instantiation { get; set; }

		[JsonProperty(PropertyName = "propertyClasses")]
		[BsonElement("propertyClasses")]
		public List<Key> PropertyClasses { get; set; }

		[JsonProperty(PropertyName = "subjectClasses")]
		[BsonElement("subjectClasses")]
		public List<Key> SubjectClasses { get; set; }

		[JsonProperty(PropertyName = "objectClasses")]
		[BsonElement("objectClasses")]
		public List<Key> ObjectClasses { get; set; }

			
	}
}
