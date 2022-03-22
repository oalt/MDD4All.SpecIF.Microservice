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

            IConfiguration configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                              .AddJsonFile("appsettings.json", optional: false)
                                                              .AddCommandLine(args)
                                                              .AddEnvironmentVariables()
                                                              .Build();

          //  Used to persist settings set via args.Otherwise these are only available during the ServiceStarter
            Environment.SetEnvironmentVariable("https_port", configuration.GetValue<string>("https_port"));
            Environment.SetEnvironmentVariable("http_port", configuration.GetValue<string>("http_port"));
            Environment.SetEnvironmentVariable("httpRedirection", configuration.GetValue<string>("httpRedirection"));
            Environment.SetEnvironmentVariable("EaConnectionString", configuration.GetValue<string>("EaConnectionString"));
            Environment.SetEnvironmentVariable("dataSource", configuration.GetValue<string>("dataSource"));
            Environment.SetEnvironmentVariable("dataConnection", configuration.GetValue<string>("dataConnection"));
            Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path", configuration.GetValue<string>("ASPNETCORE_Kestrel__Certificates__Default__Path"));
            Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password", configuration.GetValue<string>("ASPNETCORE_Kestrel__Certificates__Default__Password"));



            ServiceStarter serviceStarter = new ServiceStarter(configuration);

            serviceStarter.Start();
        }


    }
}
