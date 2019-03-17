/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class Hierarchy : SpecIfBaseElement
	{
		[JsonProperty(PropertyName = "class")]
		[BsonElement("class")]
		public Key HierarchyClass { get; set; }

		[JsonProperty(PropertyName = "properties")]
		[BsonElement("properties")]
		public List<Property> Properties { get; set; }

		[JsonProperty(PropertyName = "nodes")]
		[BsonIgnore]
		public List<Node> Nodes { get; set; }

		private List<Key> _nodeReferences;

		[JsonIgnore]
		[BsonElement("nodes")]
		public List<Key> NodeReferences
		{
			get
			{
				List<Key> result = new List<Key>();

				if (Nodes != null && Nodes.Count > 0)
				{
					foreach (Node node in Nodes)
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
	}
}
