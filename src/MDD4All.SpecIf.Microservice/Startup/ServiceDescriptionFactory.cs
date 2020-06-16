using MDD4All.SpecIF.DataModels.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDD4All.SpecIf.Microservice.Startup
{
    public class ServiceDescriptionFactory
    {

        public static ISpecIfServiceDescription Create(string type, string serverUrl = null)
        {
            ISpecIfServiceDescription result = null;

            int port = 888;
            string serviceAddress = "https://localhost";

            if (serverUrl != null)
            {
                Uri uri = new Uri(serverUrl);
                port = uri.Port;
                serviceAddress = uri.Scheme + "://" + uri.Host;

            }

            switch (type)
            {
              
                case "jira":
                    // Consul
                    
                    result = new SpecIfServiceDescription()
                    {
                        ID = "{DFFE5123-E1D0-4E24-B37C-8CF019BEB7EE}",
                        DataRead = true,
                        DataWrite = true,
                        MetadataRead = false,
                        MetadataWrite = false,
                        ServiceName = "SpecIF-Jira-Service",
                        ServiceDescription = "This service provides SpecIF data from a Jira server.",
                        IconURL = "/images/jira_logo.png",
                        Tags = new List<string> { "SpecIF-API", "Jira" }
                    };

                    break;

                case "integration":

                    result = new SpecIfServiceDescription()
                    {
                        ID = "{F8B21340-B442-4040-BEFE-CF455BABB3A5}",
                        DataRead = true,
                        DataWrite = true,
                        MetadataRead = true,
                        MetadataWrite = true,
                        ServiceName = "SpecIF-Integration-Service",
                        ServiceDescription = "This service integrates multiple SpecIF API services.",
                        IconURL = "/images/DataIntegration.png",
                        Tags = new List<string> { "SpecIF-API", "Integration" }
                    };

                    break;

                case "ea":

                    result = new SpecIfServiceDescription()
                    {
                        ID = "{E3FB9ADE-F348-427C-94B0-97384043BEBF}",
                        DataRead = true,
                        DataWrite = false,
                        MetadataRead = false,
                        MetadataWrite = false,
                        ServiceName = "SpecIF-EA-Service",
                        ServiceDescription = "This service provides SpecIF data from Enterprise Architect.",
                        IconURL = "/images/EnterpriseArchitect_logo.jpg",
                        Tags = new List<string> { "SpecIF-API", "EA" }
                    };

                    break;

                case "file":
                    result = new SpecIfServiceDescription()
                    {
                        ID = "{606D0B77-4429-4443-A362-9ADE7DA77321}",
                        DataRead = true,
                        DataWrite = true,
                        MetadataRead = true,
                        MetadataWrite = false,
                        ServiceName = "SpecIF-File-Service",
                        ServiceDescription = "This service provides SpecIF data from SpecIF files.",
                        IconURL = "/images/MongoDB_Logo.png",
                        Tags = new List<string> { "SpecIF-API", "File" }
                    };
                    break;

                case "mongodb":
                default:
                

                    result = new SpecIfServiceDescription()
                    {
                        ID = "{67FE892C-7EB1-45AD-9259-6BE910841A3A}",
                        DataRead = true,
                        DataWrite = true,
                        MetadataRead = true,
                        MetadataWrite = false,
                        ServiceName = "SpecIF-MongoDB-Service",
                        ServiceDescription = "This service provides SpecIF data from a MongoDB backend.",
                        IconURL = "/images/MongoDB_Logo.png",
                        Tags = new List<string> { "SpecIF-API", "MongoDB" }
                    };

                break;
            }

            result.ServiceAddress = serviceAddress;
            result.ServicePort = port;

            return result;
        }

    }
}
