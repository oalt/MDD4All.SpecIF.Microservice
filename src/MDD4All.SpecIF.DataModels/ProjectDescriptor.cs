/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;


namespace MDD4All.SpecIF.DataModels
{
    public class ProjectDescriptor : SpecIfElement
    {
        public ProjectDescriptor()
        {

        }

        public ProjectDescriptor(ProjectDescriptor projectDescriptor)
        {
            ID = projectDescriptor.ID;
            Title = projectDescriptor.Title;
            Description = projectDescriptor.Description;
            IsExtension = projectDescriptor.IsExtension;
            Generator = projectDescriptor.Generator;
            GeneratorVersion = projectDescriptor.GeneratorVersion;
            Rights = projectDescriptor.Rights;
            CreatedAt = projectDescriptor.CreatedAt;
            CreatedBy = projectDescriptor.CreatedBy;
        }

        [BsonElement("schema")]
        [JsonProperty(PropertyName = "$schema", Order = 1)]
        public object Schema { get; set; } = "https://specif.de/v1.0/schema.json";

        /// <summary>
        /// Project ID.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [JsonProperty(PropertyName = "id", Order = 2)]
        public string ID { get; set; } = SpecIfGuidGenerator.CreateNewSpecIfGUID();

        [JsonProperty(PropertyName = "title", Order = 3)]
        public object Title { get; set; } = new Value();

        [JsonProperty(PropertyName = "description", Order = 4)]
        public object Description { get; set; }

        [JsonProperty(PropertyName = "isExtension", Order = 5)]
        public bool IsExtension { get; set; } = false;

        //[JsonProperty(PropertyName = "specifVersion", Order = 6)]
        //public string SpecifVersion { get; set; } = "1.0";

        [JsonProperty(PropertyName = "generator", Order = 7)]
        public string Generator { get; set; }

        [JsonProperty(PropertyName = "generatorVersion", Order = 8)]
        public string GeneratorVersion { get; set; }

        [JsonProperty(PropertyName = "rights", Order = 9)]
        public Rights Rights { get; set; }

        [JsonProperty(PropertyName = "createdAt", Order = 10)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty(PropertyName = "createdBy", Order = 11)]
        public Creator CreatedBy { get; set; }
    }
}
