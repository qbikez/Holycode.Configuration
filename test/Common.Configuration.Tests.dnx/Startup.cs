﻿
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Owin.Security.SiteAuthentication;
using Common.Owin.vnext;
using Newtonsoft.Json;
using Owin;
using Microsoft.Extensions.DependencyInjection;
using Holycode.Performance;
using log4net;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace Common.Configuration
{

    public class Startup
    {
        protected static readonly
            ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IApplicationEnvironment _appEnv;
        private readonly IHostingEnvironment _hostEnv;



        public Startup(IApplicationEnvironment appEnv, IHostingEnvironment hostEnv)
        {
            _appEnv = appEnv;
            _hostEnv = hostEnv;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMetric>(s => new Holycode.Performance.NewRelic.NrMetrics("vnext-test-2"));

        }

        public void Configure(IApplicationBuilder app, IServiceProvider svc, IApplicationEnvironment env)
        {
            var metric = svc.GetRequiredService<IMetric>();
            Metrics.Configure(metric);
            var r = new Random();

            var appName = "tests.vnext";

            var baseCfg = ConfigFactory.CreateConfigSource(env.ApplicationBasePath);
            baseCfg.AddJsonFile("Config.json");
            baseCfg.AddJsonFile($"Config.{_hostEnv.EnvironmentName}.json", optional: true);

            IConfiguration cfg = baseCfg.Build();
            //baseCfg.AddWebConfig();
            // this is explicitly for old net451 apps
            // cfg.ExtractConnectionStrings(ConfigurationManager.ConnectionStrings);
            cfg.ConfigureLog4net(appName, logRootPath: cfg.BasePath() + "/log");

            app.LogAndMeasureRequests();
            app.LogStats(threadSafe: true);

            log.InfoFormat($"{appName} startup");

            ReqStatsLogger.AddListener(e =>
            {
                Metrics.RecordMetric("Custom/vnext-tests/meanReqTime", (float)e.MeanTime);
                Metrics.RecordMetric("Custom/vnext-tests/inPerSec", (float)e.InPerSec);
                Metrics.RecordMetric("Custom/vnext-tests/outPerSec", (float)e.OutPerSec);
            });

            //app.UseResponseBuffer();

            app.UseDeveloperExceptionPage();


            app.Run(async context =>
            {
                Metrics.RecordMetric("External/dummy.service.1/Net::Http::POST", 1);
                Metrics.RecordMetric("Custom/dummy.service.1/Net::Http::POST", 1);
                Metrics.RecordMetric("Custom/test1", 99);
                Metrics.RecordEvent("Customevent1", new Dictionary<string, object>() { { "timestamp", DateTimeOffset.Now } });
                Metrics.ReportError("this is an error!");

                var redis = cfg.GetConnectionStringValue("redis");
                var conn1 = cfg.GetConnectionStringValue("test");
                var conn = ConfigurationManager.ConnectionStrings["test"];
                Trace.WriteLine(conn?.ConnectionString);
                conn = ConfigurationManager.ConnectionStrings["test1"];
                Trace.WriteLine(conn?.ConnectionString);

                var obj = GetResult(cfg);
                var s = JsonConvert.SerializeObject(obj, Formatting.Indented);

                Console.WriteLine(s);

                var bytes = Encoding.UTF8.GetBytes(s);
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                

            });
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        }

        public object GetResult(IConfiguration cfg)
        {

            var obj = new
            {
                web_config_test = cfg.Get("test"),
                web_config_sub_test = cfg.Get("test:sub"),
                local_json = cfg.Get("localjsontest"),
                local_json_sub = cfg.Get("localjson:test"),
                env = cfg.Get("env"),
                appsettings = ConfigurationManager.AppSettings.AllKeys.ToDictionary(s => s, s => ConfigurationManager.AppSettings[s]),
                //config = cfg.AsDictionaryNested(),
            };

            return obj;
        }
    }
}


