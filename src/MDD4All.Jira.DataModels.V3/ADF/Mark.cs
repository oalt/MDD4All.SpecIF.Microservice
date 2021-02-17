/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels.V3.ADF
{
    public class Mark
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("attrs")]
        public Attributes Attrs { get; set; }
    }

    
}
