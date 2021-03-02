/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataIntegrator.Contracts
{
    public interface ISpecIfEventService
    {
        List<Resource> GetReceivedSpecIfEvents();
    }
}
