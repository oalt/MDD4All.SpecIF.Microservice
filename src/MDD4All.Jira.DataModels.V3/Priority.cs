/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.Jira.DataModels.V3.Converters;
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Priority
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("iconUrl")]
        public Uri IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
    }
}
