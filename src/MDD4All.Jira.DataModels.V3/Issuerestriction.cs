/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Issuerestriction
    {
        [JsonProperty("issuerestrictions")]
        public Timetracking Issuerestrictions { get; set; }

        [JsonProperty("shouldDisplay")]
        public bool ShouldDisplay { get; set; }
    }
}
