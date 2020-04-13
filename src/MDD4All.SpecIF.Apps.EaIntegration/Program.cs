using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.MongoDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAPI = EA;

namespace MDD4All.SpecIF.Apps.EaIntegration
{
    class Program
    {

        private void Start()
        {
            EnterpriseArchitectStarter enterpriseArchitectStarter = new EnterpriseArchitectStarter();

            EAAPI.Repository repository = enterpriseArchitectStarter.Start(@"d:\alto_daten\EA\KafkaIntegrationTest.eapx");

            if(repository != null)
            {
                Debug.WriteLine("Repository open.");

                ISpecIfMetadataReader metadataReader = new SpecIfMongoDbMetadataReader("mongodb://localhost:27017");

                EaKafkaIntegrator integrator = new EaKafkaIntegrator(repository, metadataReader);
            }

        }

        static void Main(string[] args)
        {
            Program program = new Program();

            program.Start();

            Console.ReadLine();
        }
    }
}
