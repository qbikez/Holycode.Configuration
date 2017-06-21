using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration
{
    public static partial class ConnectionStringsExtenisons
    {
        ///<summary>
        /// this method causes conflict with same method from Microsoft.Extensions.Configuration.Abstractions
        /// and it's intentional. GetConnectionStringValue handles also structured entries, like:
        /// "connectionStrings" {
        ///   "MyDb": {
        ///     "connectionString": "Application Name=xxx;Data Source=db1.example.org;Initial Catalog=my-db;",
        ///     "provider": "System.Data.SqlClient"
        ///   } 
        /// }
        ///</summary>
        public static string GetConnectionString(this IConfiguration cfg, string name) => GetConnectionStringValue(cfg, name);
        public static string GetConnectionStringValue(this IConfiguration cfg, string name)
        {
            var subkey = cfg.GetSection($"connectionStrings:{name}");
            if (subkey.Value == null && !subkey.GetChildren().Any())
            {
                subkey = cfg.GetSection($"{name}");
            }
            var val = subkey.Get("connectionString") ?? subkey.Value;
            return val;
            
        }

        public static string GetConnectionStringSecureValue(this IConfiguration cfg, string name)
        {
            var connStr = cfg.GetConnectionStringValue(name);
            return connStr.GetSecureValue();
        }
        
        public static string GetSecureValue(this string connStr) {
			if (connStr == null) return null;
            connStr = Regex.Replace(connStr, @"User Id\s*=.*?(;|$)", "User Id=***", RegexOptions.IgnoreCase);
            connStr = Regex.Replace(connStr, @"Password\s*=.*?(;|$)", "Password=***", RegexOptions.IgnoreCase);
            return connStr;
        }
        
    }
}

