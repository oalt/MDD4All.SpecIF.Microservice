/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Status
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public Uri IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("untranslatedName")]
        public string UntranslatedName { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("statusCategory")]
        public StatusCategory StatusCategory { get; set; }
    }
}
