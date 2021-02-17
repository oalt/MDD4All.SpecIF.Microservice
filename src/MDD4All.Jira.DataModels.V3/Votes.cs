/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Votes
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("votes")]
        public long VotesVotes { get; set; }

        [JsonProperty("hasVoted")]
        public bool HasVoted { get; set; }
    }
}
