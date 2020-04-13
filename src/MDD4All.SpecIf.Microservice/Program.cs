/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIf.Microservice.Helpers;
using MDD4All.SpecIf.Microservice.Startup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Collections.Generic;

namespace MDD4All.SpecIf.Microservice
{
	public class Program
	{
		public static void Main(string[] args)
		{
            //JwtConfigurationCreator creator = new JwtConfigurationCreator(); 
            
            ServiceStarter serviceStarter = new ServiceStarter();

            serviceStarter.Start(args);
            
        }

		
	}
}
