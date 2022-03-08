using CommandLine;

namespace MDD4All.SpecIF.Microservice.Startup
{
    public class CommandLineOptions
    {
        [Value(index: 0, Required = false, HelpText = "The service type used for data storage.")]
        public string ServiceType { get; set; } = "mongodb";

        [Option(shortName: 'a', longName: "metadataReadRequiresAuthorization", Required = false)]
        public bool MetadataReadRequiresAuthorization { get; set; }

        [Option(shortName: 'n', longName: "noHttpsRedirection")]
        public bool HttpsRedirectionActive { get; set; } = true;
    }
}
