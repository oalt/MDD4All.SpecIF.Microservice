using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels
{
    public class IssueLink
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("type")]
        public LinkType Type { get; set; }

        [JsonProperty("inwardIssue")]
        public InwardIssue InwardIssue { get; set; }
    }
}
