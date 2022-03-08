/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using CommandLine;
using MDD4All.SpecIF.Microservice.Startup;

namespace MDD4All.SpecIF.Microservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(options =>
                   {
                       ServiceStarter serviceStarter = new ServiceStarter();

                       Environment.SetEnvironmentVariable("metadataReadAuthRequired", options.MetadataReadRequiresAuthorization.ToString());

                       serviceStarter.Start(options);
                   });

        }

        
    }
}
