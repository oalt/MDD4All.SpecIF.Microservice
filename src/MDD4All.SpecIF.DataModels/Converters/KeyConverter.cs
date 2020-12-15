/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace MDD4All.SpecIF.DataModels.Converters
{
    public class KeyConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(Key) || objectType == typeof(string));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Key result = new Key();
			
			if (reader.ValueType == typeof(string))
			{
				result.ID = reader.Value.ToString();
				result.Revision = null;
			}
			else
			{
				try
				{
					var val = reader.Value;
					JObject keyJObject = JObject.Load(reader);

					result.ID = keyJObject["id"].ToString();
					result.Revision = keyJObject["revision"].ToString();
				}
				catch(Exception exception)
                {
					Debug.WriteLine(exception);
                }
			}
			
			return result;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			//Console.WriteLine("Type = " + value.GetType().ToString());

			//Console.WriteLine("Value = " + value.ToString());

			if (value is Key)
			{
				
				Key key = value as Key;

				//Console.WriteLine("value is key. " + key.ID + " " + key.Revision);
				

				try
				{

					JObject keyJObject = new JObject();

					keyJObject.Add("id", JToken.FromObject(key.ID));
					keyJObject.Add("revision", JToken.FromObject(key.Revision));

					keyJObject.WriteTo(writer);
				}
				catch(Exception exception)
				{
					Console.WriteLine("id=" + key.ID);
					Console.WriteLine("revision=" + key.Revision);
					Console.WriteLine(exception);
				}
			}
			else if (value is string)
			{
				Key key = new Key((string)value, null);

				JToken token = JToken.FromObject(key);

				token.WriteTo(writer);
			}
		}
	}
}
