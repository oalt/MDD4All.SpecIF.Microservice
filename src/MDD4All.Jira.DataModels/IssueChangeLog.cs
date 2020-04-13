using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class IssueChangeLog
    {
        [JsonProperty("startAt")]
        public int StartAt { get; set; }

        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("histories")]
        public List<History> Histories { get; set; } = new List<History>();
    }
}
