/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Progress
    {
        [JsonProperty("progress")]
        public long ProgressProgress { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }
}
