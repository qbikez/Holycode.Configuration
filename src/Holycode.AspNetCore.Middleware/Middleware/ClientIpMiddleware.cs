using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Holycode.AspNetCore.Middleware
{
    public class HttpClientLocation
    {
        public IPAddress Ip { get; private set; }
        public string BrowserLocale { get; private set; }
        public string DetectedCountryCode { get; private set; }
        public string DetectedCountryName { get; private set; }

        public HttpClientLocation(IPAddress ip, string browserLocale) {
            this.Ip = ip;
            this.BrowserLocale = browserLocale;
        }
        public HttpClientLocation(IPAddress ip, string browserLocale, string countryCode, string countryName) : this(ip, browserLocale)
        {
            this.DetectedCountryCode = countryCode;
            this.DetectedCountryName = countryName;
        }


    }
    public class ClientIpMiddleware
    {
        protected ILogger log;

        public ClientIpMiddleware(ILogger log)
        {
            this.log = log;
        }
        public async Task Invoke(HttpContext ctx, Func<Task> next)
        {
            var ipstr = GetClientIp(ctx);
            IPAddress ip = null;
            if (!IPAddress.TryParse(ipstr, out ip))
            {
                log.LogWarning("failed to recognize client ip: '{ipstr}'");
            }
            else
            {
                var location = new HttpClientLocation(
                    ip: ip,
                    browserLocale: ctx.Request.Headers["Accept-Language"]
                );
                location = FillLocation(location);
                ctx.Features.Set<HttpClientLocation>(location);
            }
            await next();
        }

        protected virtual HttpClientLocation FillLocation(HttpClientLocation loc) {
            return loc;
        }

        /// taken from https://github.com/aspnet/IISIntegration/issues/17#issuecomment-156790265
        private string GetClientIp(HttpContext ctx)
        {
            var request = ctx.Request;
            string value;
            foreach (string remoteIpHeader in remoteIpHeaders)
            {
                if (request.Headers[remoteIpHeader].Count > 0)
                {
                    value = request.Headers[remoteIpHeader];
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.Split(',')[0].Split(';')[0];
                        if (value.Contains("="))
                        {
                            value = value.Split('=')[1];
                        }
                        value = value.Trim('"');
                        if (value.Contains(":"))
                        {
                            value = value.Substring(0, value.LastIndexOf(':'));
                        }
                        log.LogDebug($"clientip: {value} (from {remoteIpHeader} header)");
                        return value.TrimStart('[').TrimEnd(']');
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var httpConn = ctx.Features.Get<IHttpConnectionFeature>();
            var ip = httpConn?.RemoteIpAddress;
            log.LogDebug($"clientip: {ip} (from RemoteIpAddress)");
            return ip?.ToString();            
        }
      
        public static string ClientIp(HttpContext ctx)
        {
            string clientip = null;
            try
            {
                var locftr = ctx.Features.Get<HttpClientLocation>();
                if (locftr != null)
                {
                    clientip = locftr?.Ip.ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return clientip;
        }

        static readonly string[] remoteIpHeaders =
            {
                "X-FORWARDED-FOR",
                "REMOTE_ADDR",
                "HTTP_X_FORWARDED_FOR",
                "HTTP_CLIENT_IP",
                "HTTP_X_FORWARDED",
                "HTTP_X_CLUSTER_CLIENT_IP",
                "HTTP_FORWARDED_FOR",
                "HTTP_FORWARDED",
                "X_FORWARDED_FOR",
                "CLIENT_IP",
                "X_FORWARDED",
                "X_CLUSTER_CLIENT_IP",
                "FORWARDED_FOR",
                "FORWARDED"
            };
    }

}
