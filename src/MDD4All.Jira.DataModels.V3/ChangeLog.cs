/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class ChangeLog
    {
        [JsonProperty("startAt")]
        public long StartAt { get; set; }

        [JsonProperty("maxResults")]
        public long MaxResults { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("histories", NullValueHandling = NullValueHandling.Ignore)]
        public List<History> Histories { get; set; }

        [JsonProperty("comments", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Comments { get; set; }

        [JsonProperty("worklogs", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Worklogs { get; set; }
    }
}
