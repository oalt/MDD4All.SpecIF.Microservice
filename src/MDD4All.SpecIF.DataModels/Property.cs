/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Helpers;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
	public class Property : SpecIfBaseElement
	{
		public Property()
		{
			Value = new Value();
		}

        public Property(string title, string classID, string value, string id, DateTime changedAt, string changedBy)
        {
            Title = new Value(title);

            PropertyClass = new Key(classID, SpecIfGuidGenerator.CreateNewSpecIfGUID());
            Value = new Value
            {
                LanguageValues = new List<LanguageValue>
                {
                    new LanguageValue
                    {
                        Text = value
                    }
                }
            };

            ID = id;

            ChangedAt = changedAt;
            ChangedBy = changedBy;
        }

		[JsonProperty(PropertyName = "class", Order = -95)]
		[BsonElement("class")]
		public Key PropertyClass { get; set; }

		[JsonProperty(PropertyName = "value", Order = -97)]
		[BsonElement("value")]
		public Value Value
		{
			get; set;
		}
			
	}
}
