using System.Collections.Generic;

namespace Holycode.Configuration.Conventions
{
    class DefaultConvention
    {
        public static IEnumerable<IConfigSourceConvention> Get(string applicationBasePath, bool optional, string environment = null)
        {
            var conventions = new List<EnvJsonConvention>();

            conventions.Add(new EnvJsonConvention(applicationBasePath, environmentName: environment)
            {
                MainConfigFile = "env.json|config/env.json",
                IsMainConfigOptional = optional || environment != null,
                IsEnvSpecificConfigOptional = optional || environment == null
            });
            // new EnvJsonConvention(applicationBasePath, environmentName: environment) {
            //     MainConfigFile = "config/env.json",
            //     IsMainConfigOptional = optional || environment != null,
            //     IsEnvSpecificConfigOptional = optional || environment == null
            // }


            if (environment == null)
            {
                conventions.Add(new EnvJsonConvention(applicationBasePath, environmentName: environment)
                {
                    MainConfigFile = "env.default.json|config/env.default.json",
                    IsMainConfigOptional = optional,
                    IsEnvSpecificConfigOptional = true
                });
                //  conventions.Add(new EnvJsonConvention(applicationBasePath, environmentName: environment) {
                //     MainConfigFile = "config/env.default.json",
                //     IsMainConfigOptional = optional,
                //     IsEnvSpecificConfigOptional = true
                // });
            }
            return conventions;
        }
    }
}