using Holycode.Configuration.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using SerilogWeb.Classic;
using SerilogWeb.Classic.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Holycode.Configuration.Serilog
{
    public class SerilogWeb
    {
        /// <summary>
        /// use like this: Initialize(config, System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath, System.Web.Hosting.HostingEnvironment.MapPath("~"));
        /// </summary>
        /// <param name="config"></param>
        /// <param name="diskPath"></param>
        /// <param name="virtPath"></param>
        public static void Initialize(IConfiguration config, string appName, string diskPath, string virtPath)
        {
            string suffix = (virtPath?.Contains("-staging") ?? false) ? "-staging" : "";
            Log.Logger = SerilogConfiguration.CreateAndInitialize(config, appname: $"{appName}{suffix}", baseDir: diskPath)
                .Enrich.With<HttpRequestIdEnricher>()
                .Enrich.With<UserNameEnricher>()
                .Enrich.With<HttpRequestUrlEnricher>()
                .Enrich.With<HttpRequestClientHostIPEnricher>()
                .WriteTo.Logger(l => l.WriteTo.Trace())
                
                .CreateLogger();

            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.OnlyOnError;
            ApplicationLifecycleModule.RequestLoggingLevel = global::Serilog.Events.LogEventLevel.Information;

            log4net.Appender.Serilog.Configuration.Configure(useParameterExtraction: false);
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="appName"></param>
        public static void Initialize(IConfiguration config, string appName)
        {
            Initialize(config, appName, 
                diskPath: System.Web.Hosting.HostingEnvironment.MapPath("~"), 
                virtPath: System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
            );
        }

    }
}
