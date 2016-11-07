/*
using Microsoft.AspNetCore.Http;
using Serilog.Core;

namespace Holycode.Configuration.Serilog {
  public class ClientIpEnricher : ILogEventEnricher
{
    IHttpContextAccessor accessor;
    public ClientIpEnricher(IHttpContextAccessor ctxAccessor)
    {
        this.accessor = ctxAccessor;
    }
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (accessor.HttpContext != null)
        {
            var ip = ClientIp(accessor.HttpContext);
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", ip));
        }

    }
}

    

}
*/