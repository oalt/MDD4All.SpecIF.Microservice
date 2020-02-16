/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels.Converters
{
    public class AlternativeIdConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            AlternativeId result = new AlternativeId();
            

            if (reader.ValueType == typeof(string))
            {
                result.alternativeIdReferences.Add(new AlternativeIdReference()
                {
                    ID = reader.Value.ToString()
                });
            }
            else
            {
                JArray ja = JArray.Load(reader);
                List<AlternativeIdReference> values = ja.ToObject<List<AlternativeIdReference>>();

                result.alternativeIdReferences = values;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            AlternativeId val = value as AlternativeId;

            if (val.alternativeIdReferences != null)
            {
                
                JArray array = new JArray();

                foreach (AlternativeIdReference alternativeIdReference in val.alternativeIdReferences)
                {
                    array.Add(JToken.FromObject(alternativeIdReference));
                }

                array.WriteTo(writer);
                
            }
            else
            {
                JToken token = JToken.FromObject("");

                token.WriteTo(writer);
            }



        }
    }
}
