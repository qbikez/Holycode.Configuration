using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Serilog.Sinks.Elasticsearch;
using System.Collections.Generic;

namespace Holycode.Configuration.Serilog.Enrichers
{
    public class ThreadIdEnricher : ILogEventEnricher
    {
        private static readonly int _procId = System.Diagnostics.Process.GetCurrentProcess().Id;
        public ThreadIdEnricher()
        {

        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", System.Threading.Thread.CurrentThread.ManagedThreadId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadName", System.Threading.Thread.CurrentThread.Name));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ProcessId", _procId));
        }
    }


    public class StaticPropertiesEnricher : ILogEventEnricher
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



}