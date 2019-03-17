/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataModels.Converters
{
    public class ValueConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(Value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{

			Value value = new Value();
			value.LanguageValues = new List<LanguageValue>();

			if (reader.ValueType == typeof(string))
			{
				value.LanguageValues.Add(new LanguageValue()
				{
					Text = reader.Value.ToString(),
					Language = null
				});
			}
			else
			{
				JArray ja = JArray.Load(reader);
				List<LanguageValue> values = ja.ToObject<List<LanguageValue>>();

				value.LanguageValues = values;
			}
			
			return value;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Value val = value as Value;

			if (val.LanguageValues != null)
			{
				if (val.LanguageValues.Count == 1 && val.LanguageValues[0].Language == null)
				{
					JToken token = JToken.FromObject(val.LanguageValues[0].Text);

					token.WriteTo(writer);
				}
				else
				{
					

					JArray array = new JArray();

					foreach(LanguageValue languageValue in val.LanguageValues)
					{
						array.Add(JToken.FromObject(languageValue));
					}

					array.WriteTo(writer);
				}
			}
			else
			{
				JToken token = JToken.FromObject("");

				token.WriteTo(writer);
			}

			

		}
	}
}
