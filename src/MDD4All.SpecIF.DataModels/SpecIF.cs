/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class SpecIF : SpecIfElement
	{
		public SpecIF()
		{
		}

		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "specifVersion")]
		public string SpecifVersion { get; set; } = "0.11.6";

		[JsonProperty(PropertyName = "generator")]
		public string Generator { get; set; }

		[JsonProperty(PropertyName = "generatorVersion")]
		public string GeneratorVersion { get; set; }

		[JsonProperty(PropertyName = "rights")]
		public Rights Rights { get; set; }

		[JsonProperty(PropertyName = "createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[JsonProperty(PropertyName = "createdBy")]
		public Creator CreatedBy { get; set; }

		// type definitions (metadata)
		[JsonProperty(PropertyName = "dataTypes")]
		public List<DataType> DataTypes { get; set; }

		[JsonProperty(PropertyName = "propertyClasses")]
		public List<PropertyClass> PropertyClasses { get; set; }

		[JsonProperty(PropertyName = "resourceClasses")]
		public List<ResourceClass> ResourceClasses { get; set; }

		[JsonProperty(PropertyName = "statementClasses")]
		public List<StatementClass> StatementClasses { get; set; }

		[JsonProperty(PropertyName = "hierarchyClasses")]
		public List<HierarchyClass> HierarchyClasses { get; set; }

		// data
		[JsonProperty(PropertyName = "resources")]
		public List<Resource> Resources { get; set; }

		[JsonProperty(PropertyName = "statements")]
		public List<Statement> Statements { get; set; }

		[JsonProperty(PropertyName = "hierarchies")]
		public List<Hierarchy> Hierarchies { get; set; }

		[JsonProperty(PropertyName = "files")]
		public List<object> Files { get; set; }
	}
}