using System;
using MDD4All.SpecIf.Microservice.Startup;

namespace MDD4All.SpecIF.Microservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceStarter serviceStarter = new ServiceStarter();

            serviceStarter.Start(args);
        }

        
    }
}
