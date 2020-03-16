/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIf.Microservice.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MDD4All.SpecIf.Microservice
{
	public class Program
	{
		public static void Main(string[] args)
		{
            //JwtConfigurationCreator creator = new JwtConfigurationCreator(); 
            BuildWebHost(args).Run();
        }

		public static IWebHost BuildWebHost(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
		}
	}
}
