/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class StatusCategory
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("colorName")]
        public string ColorName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
