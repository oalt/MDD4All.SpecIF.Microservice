/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.Jira.DataModels.V3.Converters;
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Project
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("projectTypeKey")]
        public string ProjectTypeKey { get; set; }

        [JsonProperty("simplified")]
        public bool Simplified { get; set; }

        [JsonProperty("avatarUrls")]
        public AvatarUrls AvatarUrls { get; set; }
    }
}
