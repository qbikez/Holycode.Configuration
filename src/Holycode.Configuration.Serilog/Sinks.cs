using Holycode.Configuration.Serilog.Filters;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.RollingFileAlternate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Holycode.Configuration.Serilog
{
    public partial class SerilogConfiguration 
    {
        public LoggerConfiguration UseFileSink(string template = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level} {ThreadId}] [{SourceContext}] ({ClientIp}) {Message}{NewLine}{Exception}")
        {
            var logBuilder = this;

            logBuilder.WriteTo.Logger(inner =>
                inner
                    .Filter.With(new LogContextFilter("SerilogWeb.Classic.ApplicationLifecycleModule", LogEventLevel.Warning))
                    .WriteTo.RollingFileAlternate($"{LogDir}",
                    prefix: $"{appname}-{env}-{System.Diagnostics.Process.GetCurrentProcess().Id}",
                    outputTemplate: template,
                    retainedFileCountLimit: 100, fileSizeLimitBytes: (5 << 20)
                )
            );

            return this;
        }

        public LoggerConfiguration UseElasticSink(Action<ElasticsearchSinkOptions> configureElastic = null)
        {
            var elasticSink = configuration["Logging:Sinks:Elastic"] ?? configuration["Logging:Sinks:Elastic:ConnectionString"];
            this.WriteTo.Logger(inner =>
            {
                var elasticOpts = new ElasticsearchSinkOptions(new Uri(elasticSink ?? "http://localhost:9200"))
                {
                    IndexFormat = $"logstash-{prefix}-{env}-{appname}-{{0:yyyy.MM.dd}}"
                };
                if (configureElastic != null) configureElastic(elasticOpts);
                inner.WriteTo.Elasticsearch(elasticOpts);
            });

            return this;
        }

        public LoggerConfiguration UseGraylog()
        {
            var elasticSink = configuration["Logging:Sinks:Graylog"] ?? configuration["Logging:Sinks:Graylog:ConnectionString"];
            this.WriteTo.Logger(inner =>
            {
                var elasticOpts = new ElasticsearchSinkOptions(new Uri(elasticSink ?? "http://localhost:9200"))
                {
                    IndexFormat = $"logstash-{prefix}-{env}-{appname}-{{0:yyyy.MM.dd}}"
                };
                
                inner.WriteTo.Elasticsearch(elasticOpts);
            });

            return this;
        }


    }
}
