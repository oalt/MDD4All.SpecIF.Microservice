/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels.V3
{
    public class User
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("accountId")]
        public string AccountID { get; set; }

        [JsonProperty("avatarUrls")]
        public AvatarUrls AvatarURLs { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }
    }
}
