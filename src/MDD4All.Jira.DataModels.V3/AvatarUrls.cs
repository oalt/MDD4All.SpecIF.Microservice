/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class AvatarUrls
    {
        [JsonProperty("48x48")]
        public Uri The48X48 { get; set; }

        [JsonProperty("24x24")]
        public Uri The24X24 { get; set; }

        [JsonProperty("16x16")]
        public Uri The16X16 { get; set; }

        [JsonProperty("32x32")]
        public Uri The32X32 { get; set; }
    }
}
