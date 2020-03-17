/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class SpecIF : ProjectDescriptor
	{
		public SpecIF()
		{
		}

		// type definitions (metadata)
        [BsonIgnore]
		[JsonProperty(PropertyName = "dataTypes", Order = 20)]
		public List<DataType> DataTypes { get; set; } = new List<DataType>();

        [BsonIgnore]
        [JsonProperty(PropertyName = "propertyClasses", Order = 21)]
		public List<PropertyClass> PropertyClasses { get; set; } = new List<PropertyClass>();

        [BsonIgnore]
        [JsonProperty(PropertyName = "resourceClasses", Order = 22)]
		public List<ResourceClass> ResourceClasses { get; set; } = new List<ResourceClass>();

        [BsonIgnore]
        [JsonProperty(PropertyName = "statementClasses", Order = 23)]
		public List<StatementClass> StatementClasses { get; set; } = new List<StatementClass>();

        // data
        [BsonIgnore]
        [JsonProperty(PropertyName = "resources", Order = 24)]
		public List<Resource> Resources { get; set; } = new List<Resource>();

        [BsonIgnore]
        [JsonProperty(PropertyName = "statements", Order = 25)]
		public List<Statement> Statements { get; set; } = new List<Statement>();

        
        [JsonProperty(PropertyName = "hierarchies", Order = 26)]
		[BsonIgnore]
		public List<Node> Hierarchies { get; set; } = new List<Node>();


        [BsonIgnore]
        [JsonProperty(PropertyName = "files", Order = 27)]
		public List<File> Files { get; set; }
	}
}