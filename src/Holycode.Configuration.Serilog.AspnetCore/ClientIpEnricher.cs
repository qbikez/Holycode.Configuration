/*
namespace Holycode.Configuration.Serilog {
  public class ClientIpEnricher : Serilog.Core.ILogEventEnricher
{
    IHttpContextAccessor accessor;
    public ClientIpEnricher(IHttpContextAccessor ctxAccessor)
    {
        this.accessor = ctxAccessor;
    }
    public void Enrich(LogEvent logEvent, Serilog.Core.ILogEventPropertyFactory propertyFactory)
    {
        if (accessor.HttpContext != null)
        {
            var ip = ClientLocationMiddleware.ClientIp(accessor.HttpContext);
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", ip));
        }

    }
}

    

}
*/