/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.Jira.DataModels.V3.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class History
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("author")]
        public Creator Author { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}
