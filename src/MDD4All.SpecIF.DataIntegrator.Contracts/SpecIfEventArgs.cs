/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using System;

namespace MDD4All.SpecIF.DataIntegrator.Contracts
{
    public class SpecIfEventArgs : EventArgs
    {
        public Resource specIfEvent { get; set; }
    }
}
