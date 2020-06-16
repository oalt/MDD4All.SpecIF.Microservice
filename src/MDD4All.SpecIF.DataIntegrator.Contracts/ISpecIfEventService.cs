using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.DataIntegrator.Contracts
{
    public interface ISpecIfEventService
    {
        List<Resource> GetReceivedSpecIfEvents();
    }
}
