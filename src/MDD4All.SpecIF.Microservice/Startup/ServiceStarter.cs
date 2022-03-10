/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Service;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class ServiceStarter
    {
        public static string Type { get; private set; }
        private static Logger _logger = LogManager.GetCurrentClassLogger();
       
        public IConfiguration Configuration { get;  }

        public ServiceStarter(IConfiguration configuration)
        {
         Configuration = configuration;
        }


        public void Start()
        {
            IWebHost webHost = null;

            Type = Configuration.GetValue<string>("dataSource");
            
           bool metadataReadAuthRequired = Configuration.GetValue<bool>("metadataReadAuthRequired");
            Environment.SetEnvironmentVariable("metadataReadAuthRequired", metadataReadAuthRequired.ToString());

            webHost = CreateWebHost();
            

            if (webHost != null)
            {

                webHost.Start();

                ILogger<ServiceStarter> logger = webHost.Services.GetRequiredService<ILogger<ServiceStarter>>();

                ICollection<string> addresses = webHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses;

                if (addresses.Count > 0) // Consul registration
                {
                    ISpecIfServiceDescription serviceDescription = ServiceDescriptionFactory.Create(Type, addresses.ToArray()[0]);

                    if (serviceDescription != null)
                    {
                        ServiceRegistrator serviceRegistrator = new ServiceRegistrator();
                        serviceRegistrator.RegisterService(serviceDescription, logger);
                    }
                }



                webHost.WaitForShutdown();
            }
        }

        private IWebHost CreateWebHost()
        {
            IWebHost result = null;

            
            string hostingCertificate = (Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path"));
           
            bool test = Configuration.GetValue<bool>("metadataReadAuthRequired");
            bool hostHttps = true;
            string serviceType = Type.ToLower();
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder().UseConfiguration(Configuration);

            if (Configuration.GetValue<bool>("httpOnly") == true 
                || !System.Diagnostics.Debugger.IsAttached && File.Exists(hostingCertificate) != true)
            {
                hostHttps = false;
            }

            if (hostHttps == false)
            {              
                _logger.Warn("SSL certificate not found. Check path and password. HTTPS deactivated. Environment variables:" +
                             "ASPNETCORE_Kestrel__Certificates__Default__Path  ASPNETCORE_Kestrel__Certificates__Default__Password ");
            }

            if (serviceType == "mongodb" || serviceType == null)
            {

                webHostBuilder.UseStartup<MongoDbStartup>();
                result = HostUrls(hostHttps, 888, 887, webHostBuilder);
            }
            else if (serviceType == "jira")
            {
                webHostBuilder.UseStartup<JiraStartup>();
                result = HostUrls(hostHttps, 999, 998, webHostBuilder);
            }
            else if (serviceType == "integration")
            {
                webHostBuilder.UseStartup<IntegrationStartup>();
                result = HostUrls(hostHttps, 555, 554, webHostBuilder);
            }
            else if (serviceType == "ea")
            {
                webHostBuilder.UseStartup<EaStartup>();
                result = HostUrls(hostHttps, 444, 443, webHostBuilder);
            }
            else if (serviceType == "file")
            {
                webHostBuilder.UseStartup<FileStartup>();
                result = HostUrls(hostHttps, 666, 665, webHostBuilder);
            }

            if (result != null)
            {
                ILogger<ServiceStarter> logger = result.Services.GetRequiredService<ILogger<ServiceStarter>>();

                string urls = "";

                foreach (string url in StartupBase.Urls)
                {
                    urls += url + " ";
                }

                logger.LogInformation("Start SpecIF API [" + serviceType + "] on " + urls);
            }

            return result;
        }

        private  IWebHost HostUrls(bool hostHttps, int portHttps, int portHttp, IWebHostBuilder webHostBuilder)
        {
            IWebHost result = null;

            StartupBase.Urls = new List<string> { "https://*:" + portHttps, "http://+:" + portHttp };           

            if (!hostHttps)
            {
                StartupBase.Urls = new List<string> { "http://+:" + portHttp };
                _logger.Warn("Warning: HTTPS connection deactivated. Not secure! Consider switching to https");
            }


            result = webHostBuilder
                                .UseUrls(StartupBase.Urls.ToArray())
                                .UseKestrel()
                                .ConfigureLogging(ConfigureLoggingAction)
                               
                            .UseConfiguration(Configuration)
                                .Build();
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
