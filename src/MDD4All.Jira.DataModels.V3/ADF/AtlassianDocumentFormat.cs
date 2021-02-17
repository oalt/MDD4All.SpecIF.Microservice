/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels.V3.ADF
{
    public class AtlassianDocumentFormat
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("content")]
        public List<Content> Content { get; set; }

        public override string ToString()
        {
            string result = "";

            result += Type;

            return result; 
        }

    }


    
}
