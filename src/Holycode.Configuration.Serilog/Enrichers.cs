using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Serilog.Sinks.Elasticsearch;
using System.Collections.Generic;

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


    class StaticPropertiesEnricher : ILogEventEnricher
    {
        private Dictionary<string, object> _values;
        private Dictionary<string, LogEventProperty> _cachedProperties = new Dictionary<string, LogEventProperty>();

        public StaticPropertiesEnricher(Dictionary<string, object> values)
        {
            _values = values;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var kvp in _values)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(kvp.Key, kvp.Value));

                LogEventProperty cachedProperty = null;
                if (!_cachedProperties.TryGetValue(kvp.Key, out cachedProperty))
                {
                    cachedProperty = propertyFactory.CreateProperty(kvp.Key, kvp.Value);
                    _cachedProperties[kvp.Key] = cachedProperty;
                }

                logEvent.AddPropertyIfAbsent(cachedProperty);
            }
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