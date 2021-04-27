/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MDD4All.SpecIf.Microservice.Hubs;
using MDD4All.SpecIF.DataIntegrator.Contracts;
using MDD4All.SpecIF.DataIntegrator.KafkaListener;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.ViewModels.SpecIfEvent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    public class SpecIfEventController : Controller
    {

        

        private static int callCount = 0;

        private ISpecIfMetadataReader _metadataReader;

        private ISpecIfEventService _eventService;

        private IHubContext<SpecIfEventHub> _hubContext;

        public SpecIfEventController(ISpecIfEventService eventService, 
                                     ISpecIfMetadataReader metadataReader,
                                     IHubContext<SpecIfEventHub> hubContext)
        {
            _eventService = eventService;
            _metadataReader = metadataReader;
            _hubContext = hubContext;

            if(eventService is KafkaSpecIfEventService)
            {
                KafkaSpecIfEventService kafkaSpecIfEventService = eventService as KafkaSpecIfEventService;

                kafkaSpecIfEventService.SpecIfEventListener.SpecIfEventReceived += SpecIfEventReceived;

            }
        }

        public IActionResult Index()
        {
            List<Resource> events = _eventService.GetReceivedSpecIfEvents();

            List<SpecIfEventViewModel> dataModel = new List<SpecIfEventViewModel>();

            foreach(Resource specifEvent in events)
            {
                SpecIfEventViewModel specIfEventViewModel = new SpecIfEventViewModel(specifEvent, _metadataReader);
                dataModel.Insert(0, specIfEventViewModel);
            }

            return View(dataModel);
        }

        private void SpecIfEventReceived(object sender, SpecIfEventArgs e)
        {
            try
            {
                _hubContext.Clients.All.SendAsync("SpecIfEvent");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
    }
}