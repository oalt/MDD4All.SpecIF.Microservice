/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.IO;
using MDD4All.SpecIF.Microservice.Startup;
using Microsoft.Extensions.Configuration;

namespace MDD4All.SpecIF.Microservice
{
    public class Program
    {
        public static void Main(string[] args)
        {

            IConfiguration config = new ConfigurationBuilder()

           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false)
           .AddEnvironmentVariables()
           .AddCommandLine(args)
           .Build();

            ServiceStarter serviceStarter = new ServiceStarter(config);

            serviceStarter.Start();


        }


    }
}
