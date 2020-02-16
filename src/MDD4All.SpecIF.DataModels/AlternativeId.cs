/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels
{
    [JsonConverter(typeof(AlternativeIdConverter))]
    public class AlternativeId : SpecIfElement
    {
        [JsonIgnore]
        public List<AlternativeIdReference> alternativeIdReferences { get; set; } = new List<AlternativeIdReference>();

    }
}
