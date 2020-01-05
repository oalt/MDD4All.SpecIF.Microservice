/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MDD4All.SpecIF.DataModels.Converters
{
	public class RevisionConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(string) || objectType == typeof(int));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Revision result = new Revision();

			if (reader.ValueType == typeof(string))
			{
				result.StringValue = reader.Value.ToString();
			}
			else if(reader.ValueType == typeof(int))
			{
				result.RevsionNumber = int.Parse(reader.Value.ToString());
				result.BranchName = "main";
			}
            //else if(objectType == typeof(Revision))
            //{
            //    // Load JObject from stream
            //    JObject jObject = JObject.Load(reader);

                
            //    //Create a new reader for this jObject, and set all properties to match the original reader.
            //    JsonReader jObjectReader = jObject.CreateReader();
            //    jObjectReader.Culture = reader.Culture;
            //    jObjectReader.DateParseHandling = reader.DateParseHandling;
            //    jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
            //    jObjectReader.FloatParseHandling = reader.FloatParseHandling;

            //    // Populate the object properties
            //    serializer.Populate(jObjectReader, result);

            
            //}

			return result;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			//Console.WriteLine("Type = " + value.GetType().ToString());

			//Console.WriteLine("Value = " + value.ToString());

			if (value is Revision)
			{

				Revision revision = value as Revision;

				JToken token = JToken.FromObject(revision.StringValue);

				token.WriteTo(writer);
			}
		}
	}
}
