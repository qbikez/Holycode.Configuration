using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Serilog.Sinks.Elasticsearch;

namespace Holycode.Configuration.Serilog
{
    class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", System.Threading.Thread.CurrentThread.ManagedThreadId));
        }
    }


    class LoggerDomainEnricher : ILogEventEnricher
    {
        private string _appname;
        public LoggerDomainEnricher(string appname)
        {
            _appname = appname;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("domain", _appname));
        }
    }

    class IpFilter : ILogEventFilter
    {
        private LoggingLevelSwitch logLevel;
        public string Ip { get; set; }

        public IpFilter(LoggingLevelSwitch logLevel, string ip)
        {
            this.logLevel = logLevel;
            this.Ip = ip;
        }
        public bool IsEnabled(LogEvent ev)
        {
            if (Ip != null)
            {
                LogEventPropertyValue ctx;
                if (ev.Properties.TryGetValue("ClientIp", out ctx))
                {
                    if (((ScalarValue)ctx).Value.ToString() == Ip) return true;
                }
            }
            return ev.Level >= logLevel.MinimumLevel;
        }
    }

}