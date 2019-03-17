/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Converters;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	[JsonConverter(typeof(ValueConverter))]
    public class Value : SpecIfElement
	{
		public Value()
		{
			LanguageValues = new List<LanguageValue>();
		}

		[BsonElement("languageValues")]
		public List<LanguageValue> LanguageValues { get; set; }
	}
}
