/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Service;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels.BaseTypes
{
	public class SpecIfElement
	{
		[JsonIgnore]
		[BsonIgnore]
		public ISpecIfServiceDescription DataSource { get; set; }
	}
}
