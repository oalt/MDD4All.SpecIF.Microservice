/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MDD4All.SpecIF.DataModels.Converters
{
    public class KeyConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(Key));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Key result = new Key();
			
			if (reader.ValueType == typeof(string))
			{
				result.ID = reader.Value.ToString();
				result.Revision = 1;
			}
			else
			{
				JObject keyJObject = JObject.Load(reader);

				result.ID = keyJObject["id"].ToString();
				result.Revision = int.Parse(keyJObject["revision"].ToString());
			}
			
			return result;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Key key = value as Key;

			if (!string.IsNullOrEmpty(key.ID))
			{
				JToken token = JToken.FromObject(key);

				token.WriteTo(writer);
			}
		}
	}
}
