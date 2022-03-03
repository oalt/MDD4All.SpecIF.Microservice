/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.IO;
using MDD4All.SpecIF.Microservice.Startup;

namespace MDD4All.SpecIF.Microservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceStarter serviceStarter = new ServiceStarter();
            if (args.Length > 1 && args[1] == "true")
            {
                Environment.SetEnvironmentVariable("accessRead", "true");
            }
            if (args.Length > 1 && args[1] == "false" )
            {
                Environment.SetEnvironmentVariable("accessRead", "false");
            }
            if (args.Length <= 1)
            {
                Environment.SetEnvironmentVariable("accessRead", "true");
            }
            if (args.Length == 3 && args[2] == "httpOnly")
            {
                Environment.SetEnvironmentVariable("httpOnly", "httpOnly");
            }
            


            serviceStarter.Start(args);
        }

        
    }
}
