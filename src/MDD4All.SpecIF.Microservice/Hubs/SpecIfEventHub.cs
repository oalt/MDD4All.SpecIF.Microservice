/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.Microservice.Hubs
{
    public class SpecIfEventHub : Hub
    {

        public async Task SpecIfEventReceived(string specifEvent)
        {
            await Clients.All.SendAsync("SpecIfEvent", specifEvent);
        }
    }
}
