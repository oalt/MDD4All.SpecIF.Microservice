/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels.V3.ADF
{
    public class Content
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("content")]
        public List<Content> Contents { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("marks")]
        public List<Mark> Marks { get; set; }

        public override string ToString()
        {
            return Type;
        }
    }

    
}
