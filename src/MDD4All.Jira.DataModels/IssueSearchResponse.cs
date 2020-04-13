using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels
{
    public class IssueSearchResponse
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("startAt")]
        public int StartAt { get; set; }

        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("issues")]
        public List<Issue> Issues { get; set; }
    }
}
