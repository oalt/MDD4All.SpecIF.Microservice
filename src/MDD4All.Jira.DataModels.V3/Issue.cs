using MDD4All.Jira.DataModels.V3.Converters;
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Issue
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("changelog")]
        public ChangeLog ChangeLog { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }
    }
}
