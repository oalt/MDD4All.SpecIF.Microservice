/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class NonEditableReason
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
