/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using MDD4All.SpecIF.Microservice.Startup;

namespace MDD4All.SpecIF.Microservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceStarter serviceStarter = new ServiceStarter();
            //2nd argument for allow anonymous
            serviceStarter.Start(args);
        }

        
    }
}
