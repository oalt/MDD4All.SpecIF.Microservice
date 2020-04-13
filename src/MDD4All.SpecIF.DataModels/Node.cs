/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class Node : SpecIfBaseElement
	{
		
		public Node() : base()
		{
			Nodes = new List<Node>();
			ChangedAt = DateTime.Now;
		}

		[JsonProperty(PropertyName = "resource")]
		[BsonIgnore]
		public object ResourceObject
		{
			get
			{
				return JObject.FromObject(_resourceReference);
			}

			set
			{
				if(value is string)
				{
					_resourceReference.ID = value as string;
				}
				else if(value is JObject)
				{
					JObject resourceJObject = value as JObject;

					_resourceReference = new Key()
					{
						ID = resourceJObject["id"].ToString(),
						Revision = resourceJObject["revision"].ToString()
					};
				}
                else if(value is Key)
                {
                    _resourceReference = value as Key;
                }
				
			}
		}

		[JsonIgnore]
		[BsonIgnore]
		public string ResourceString
		{
			get
			{
				return _resourceReference.ID;
			}

			set
			{
				_resourceReference.ID = value;
				//_resourceReference.Revision = Key.LATEST_REVISION;
			}
		}

		private Key _resourceReference = new Key() { ID = "" };

		[JsonIgnore]
		[BsonElement("resource")]
		public Key ResourceReference
		{
			get
			{
				return _resourceReference;
			}

			set
			{
				_resourceReference = value;				
			}
		}

		[JsonIgnore]
		[BsonElement("isHierarchyRoot")]
		public bool IsHierarchyRoot { get; set; } = false;

		[JsonProperty(PropertyName = "nodes")]
		[BsonIgnore]
		public List<Node> Nodes { get; set; }

		private List<Key> _nodeReferences = new List<Key>();

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
