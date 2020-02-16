/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;


namespace MDD4All.SpecIF.DataModels
{
    public class ProjectDescriptor
    {
        [JsonProperty(PropertyName = "id")]
        [BsonElement("id")]
        public string ID { get; set; } = "";

        [JsonProperty(PropertyName = "changedAt")]
        [BsonElement("changedAt")]
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
