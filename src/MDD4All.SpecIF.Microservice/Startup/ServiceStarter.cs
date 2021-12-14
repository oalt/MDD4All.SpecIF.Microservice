/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Service;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class ServiceStarter
    {

        public static string Type { get; private set; }

        public void Start(string[] args)
        {
            IWebHost webHost = null;

            Type = "mongodb";

            if (args != null && args.Length > 0)
            {
                ServiceStarter.Type = args[0];
                webHost = CreateWebHost(args, ServiceStarter.Type);
            }
            else
            {
                webHost = CreateWebHost(args, null);
            }

            if (webHost != null)
            {   
                
                
                
                
                webHost.Start();

                ILogger<ServiceStarter> logger = webHost.Services.GetRequiredService<ILogger<ServiceStarter>>();

                ICollection<string> addresses = webHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses;

                if (addresses.Count > 0) // Consul registration
                {
                    ISpecIfServiceDescription serviceDescription = ServiceDescriptionFactory.Create(ServiceStarter.Type, addresses.ToArray()[0]);

                    if (serviceDescription != null)
                    {
                        ServiceRegistrator serviceRegistrator = new ServiceRegistrator();
                        serviceRegistrator.RegisterService(serviceDescription, logger);
                    }
                }



                webHost.WaitForShutdown();
            }
        }

        private IWebHost CreateWebHost(string[] args, string type)
        {
            IWebHost result = null;



            if (type == "mongodb" || type == null )
            {
                Startup.StartupBase.Urls = new List<string> { "https://127.0.0.1:888", "http://127.0.0.1:887" };

                result = WebHost.CreateDefaultBuilder(args)

                                                    .UseStartup<MongoDbStartup>()
                                                   .UseUrls(Startup.StartupBase.Urls.ToArray())
                                                   .UseKestrel()
                                                   //.UseKestrel(options =>
                                                   //{
                                                   //    options.Listen(IPAddress.Loopback, 887);
                                                   //    options.Listen(IPAddress.Loopback, 888, listenOptions =>
                                                   //    {
                                                   //        listenOptions.UseHttps("MDD4All.SpecIf.Microservice.pfx", "YourSecurePassword");
                                                   //    });
                                                   //}
                                                   //    )
                                                    .ConfigureLogging(ConfigureLoggingAction)
                                                    .Build();
            }
            else if (type == "jira")
            {
                Startup.StartupBase.Urls = new List<string> { "https://127.0.0.1:999", "http://127.0.0.1:998" };

                result = WebHost.CreateDefaultBuilder(args)
                                                    .UseStartup<JiraStartup>()
                                                    .UseUrls(Startup.StartupBase.Urls.ToArray())
                                                    .ConfigureLogging(ConfigureLoggingAction)
                                                    .Build();


            }
            else if(type == "integration")
            {
                Startup.StartupBase.Urls = new List<string> { "https://127.0.0.1:555", "http://127.0.0.1:554" };

                result = WebHost.CreateDefaultBuilder(args)                                                       
                                                    .UseStartup<IntegrationStartup>()
                                                    .UseUrls(Startup.StartupBase.Urls.ToArray())
                                                    .ConfigureLogging(ConfigureLoggingAction)
                                                    .Build();
            }
            else if (type == "ea")
            {
                Startup.StartupBase.Urls = new List<string> { "https://127.0.0.1:444", "http://127.0.0.1:443" };

                result = WebHost.CreateDefaultBuilder(args)
                                                    .UseStartup<EaStartup>()
                                                    .UseUrls(Startup.StartupBase.Urls.ToArray())
                                                    .ConfigureLogging(ConfigureLoggingAction)
                                                    .Build();



            }
            else if(type == "file")
            {
                Startup.StartupBase.Urls = new List<string> { "https://127.0.0.1:666", "http://127.0.0.1:665" };

                result = WebHost.CreateDefaultBuilder(args)
                                                    .UseStartup<FileStartup>()
                                                    .UseUrls(Startup.StartupBase.Urls.ToArray())
                                                    .ConfigureLogging(ConfigureLoggingAction)
                                                    .Build();


            }

            if(result != null)
            {
                ILogger<ServiceStarter> logger = result.Services.GetRequiredService<ILogger<ServiceStarter>>();

                string urls = "";
                
                foreach(string url in StartupBase.Urls)
                {
                    urls += url + " ";
                }

                logger.LogInformation("Start SpecIF API [" + type + "] on " + urls);
            }

            return result;
        }

        private void ConfigureLoggingAction(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddEventSourceLogger();
        }
    }
}
