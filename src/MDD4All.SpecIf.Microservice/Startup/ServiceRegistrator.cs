﻿using Consul;
using MDD4All.SpecIF.DataModels.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDD4All.SpecIf.Microservice.Startup
{
    public class ServiceRegistrator
    {
        public void RegisterService(ISpecIfServiceDescription specIfServiceDescription)
        {
            // register with consul
            try
            {
                ConsulClient consulClient = new ConsulClient();

                AgentServiceRegistration registration = new AgentServiceRegistration()
                {
                    ID = specIfServiceDescription.ID,
                    Name = specIfServiceDescription.ServiceName,
                    Address = specIfServiceDescription.ServiceAddress,
                    Port = specIfServiceDescription.ServicePort,
                    Tags = specIfServiceDescription.Tags.ToArray()
                };

                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                consulClient.Agent.ServiceRegister(registration).Wait();

                //lifetime.ApplicationStopping.Register(() =>
                //{

                //    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                //});
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unable to register in consul.");
            }
        }
    }
}
