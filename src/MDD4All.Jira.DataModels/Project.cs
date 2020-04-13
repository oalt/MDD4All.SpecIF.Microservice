using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Project
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("projectTypeKey")]
        public string ProjectTypeKey { get; set; }

        [JsonProperty("simplified")]
        public bool? Simplified { get; set; }

        [JsonProperty("avatarUrls")]
        public AvatarURLs AvatarURLs { get; set; }
    }
}
