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
			 
		}

		public Value(string value)
		{
            SimpleValue = value;
		}

		[BsonElement("languageValues")]
        [JsonIgnore]
		public List<LanguageValue> LanguageValues { get; set; } = new List<LanguageValue>();

        [BsonElement("simpleValue")]
        [JsonIgnore]
        public string SimpleValue { get; set; } = null;


		public string ToSimpleTextString()
		{
			string result = "";

            if(SimpleValue != null)
            {
                result = SimpleValue;
            }
            else if(LanguageValues.Count > 0)
			{
				result = LanguageValues[0].Text;
			}

			return result;
		}

		public override string ToString()
		{
			return ToSimpleTextString();
		}
	}
}
