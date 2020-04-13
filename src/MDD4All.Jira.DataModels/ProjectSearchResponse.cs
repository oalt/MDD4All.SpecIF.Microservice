using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class ProjectSearchResponse
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }

        [JsonProperty("startAt")]
        public int StartAt { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("isLast")]
        public bool IsLast { get; set; }

        [JsonProperty("values")]
        public List<Project> Values { get; set; }
    }
}
