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
		[JsonProperty(PropertyName = "subjectClasses")]
		[BsonElement("subjectClasses")]
		public List<string> SubjectClasses { get; set; }

		[JsonProperty(PropertyName = "objectClasses")]
		[BsonElement("objectClasses")]
		public List<string> ObjectClasses { get; set; }

		[JsonProperty(PropertyName = "propertyClasses")]
		[BsonElement("propertyClasses")]
		public List<Key> PropertyClasses { get; set; }		
	}
}
