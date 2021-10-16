/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;

namespace MDD4All.SpecIF.Microservice.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}