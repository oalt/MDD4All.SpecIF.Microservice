/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
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
		public string SpecifVersion { get; set; } = "0.11.7";

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

		// data
		[JsonProperty(PropertyName = "resources")]
		public List<Resource> Resources { get; set; }

		[JsonProperty(PropertyName = "statements")]
		public List<Statement> Statements { get; set; }

		[JsonProperty(PropertyName = "hierarchies")]
		[BsonIgnore]
		public List<Node> Hierarchies { get; set; }

		private List<Key> _nodeReferences;

		[JsonIgnore]
		[BsonElement("hierarchies")]
		public List<Key> NodeReferences
		{
			get
			{
				List<Key> result = new List<Key>();

				if (Hierarchies != null && Hierarchies.Count > 0)
				{
					foreach (Node node in Hierarchies)
					{
						Key nodeReference = new Key()
						{
							ID = node.ID,
							Revision = node.Revision
						};

						result.Add(nodeReference);
					}
				}
				else
				{
					result = _nodeReferences;
				}
				return result;
			}

			set
			{
				_nodeReferences = value;
			}
		}

		[JsonProperty(PropertyName = "files")]
		public List<File> Files { get; set; }
	}
}