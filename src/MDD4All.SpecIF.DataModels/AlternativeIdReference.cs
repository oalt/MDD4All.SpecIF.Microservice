/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataModels
{
    public class AlternativeIdReference
    {
        /// <summary>
        /// ID used in JSON
        /// </summary>
        [JsonProperty(PropertyName = "id", Order = -101)]
        [BsonElement("id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "revision", Order = -100)]
        [BsonElement("revision")]
        public string Revision { get; set; }

        [JsonProperty(PropertyName = "project", Order = -101)]
        [BsonElement("project")]
        public string Project { get; set; }

    }
}
