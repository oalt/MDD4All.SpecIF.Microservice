using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels
{
    public class Issue
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }

        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("changelog")]
        public IssueChangeLog ChangeLog { get; set; }
    }
}
