﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SFA.DAS.ProviderRegistrations.Web.Extensions;
using StructureMap.AspNetCore;

namespace SFA.DAS.ProviderRegistrations.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureDasAppConfiguration()
                .UseApplicationInsights()
                .UseNLog()
                .UseStructureMap()
                .UseStartup<Startup>();
    }
}