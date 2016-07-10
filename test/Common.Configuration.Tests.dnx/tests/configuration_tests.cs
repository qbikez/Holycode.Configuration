using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Should;
using Xunit;
using Microsoft.Extensions.PlatformAbstractions;

namespace Holycode.Configuration.Tests.dnx
{

    public class configuration_tests
    {
        private readonly IApplicationEnvironment _env;

        public configuration_tests()
        {
            _env = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IApplicationEnvironment>();
            //_hostingEnv = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IHostingEnvironment>();
        }

        [Fact]
        public void base_path_is_correct()
        {
            // Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length) will get dnx loader path
            var builder = ConfigFactory.CreateConfigSource(_env.ApplicationBasePath);
            //builder.SetBasePath(_env.ApplicationBasePath);
            builder.GetBasePath().ShouldEqual(_env.ApplicationBasePath);

            var props = builder.Properties;
            var provs = builder.Providers;

            var cfg = builder.Build();

            cfg.BasePath().ShouldNotBeNull();
            cfg.BasePath().ShouldEqual(_env.ApplicationBasePath);
        }

        [Fact]
        public void env_json_works()
        {
            Console.WriteLine("curdir=" + System.IO.Directory.GetCurrentDirectory());
            var basepath = System.IO.Path.GetFullPath(@".\input\reporoot1\subfolder\projectfolder");
            var builder = ConfigFactory.CreateConfigSource(basepath);

            builder.AddEnvJson(optional: false);

            var props = builder.Properties;
            var provs = builder.Providers;

            var cfg = builder.Build();

            cfg.BasePath().ShouldNotBeNull();
            cfg.BasePath().ShouldEqual(basepath);

            cfg.EnvironmentName().ShouldEqual("test-env");
            cfg.Get("test").ShouldEqual("global-test-env-value");
        }

        [Fact]
        public void local_config_works()
        {
            var basepath = System.IO.Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder");

            var builder = ConfigFactory.CreateConfigSource(basepath);
            builder.AddEnvJson(optional: false);
            builder.AddJsonFile("config.json", optional: false);
            builder.AddJsonFile($"config.{builder.EnvironmentName()}.json", optional: false);

            var props = builder.Properties;
            var provs = builder.Providers;

            var cfg = builder.Build();

            cfg.BasePath().ShouldNotBeNull();
            cfg.BasePath().ShouldEqual(basepath);

            cfg.EnvironmentName().ShouldEqual("test-env");
            cfg.Get("test").ShouldEqual("local-config-value");
        }

        //[Fact]
        [Obsolete("extracting connection strings is not supported in aspnet5 anymore")]
        public void extract_connection_strings()
        {
            var basepath = System.IO.Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder");

            var builder = ConfigFactory.CreateConfigSource(basepath);
            builder.AddEnvJson(optional: false);
            builder.AddJsonFile("config.json", optional: false);
            builder.AddJsonFile($"config.{builder.EnvironmentName()}.json", optional: false);

            var cfg = builder.Build();

            //cfg.ExtractConnectionStrings(ConfigurationManager.ConnectionStrings);

            var testConn = ConfigurationManager.ConnectionStrings["test"];
            testConn.ShouldNotBeNull();
            var test1conn = ConfigurationManager.ConnectionStrings["test1"];
            test1conn.ShouldNotBeNull();

            cfg.GetConnectionStringValue("test").ShouldEqual(testConn?.ConnectionString);
            cfg.GetConnectionStringValue("test1").ShouldEqual(test1conn?.ConnectionString);
        }


        [Fact]
        public void traverse_config_tree_no_env_vars()
        {
            var basepath = System.IO.Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder");

            var builder = ConfigFactory.CreateConfigSource(basepath, addEnvVariables: false);

            builder.AddEnvJson(optional: false);
            builder.AddJsonFile("config.json", optional: false);
            builder.AddJsonFile($"config.{builder.EnvironmentName()}.json", optional: false);

            var cfg = builder.Build();

            //cfg.ExtractConnectionStrings(ConfigurationManager.ConnectionStrings);
            var traversed = cfg.AsDictionaryNested();
            traversed.ShouldNotBeNull();
        }

        [Fact]
        public void traverse_config_tree()
        {
            var builder = ConfigFactory.CreateConfigSource(_env.ApplicationBasePath, addEnvVariables: false);

            /// VS seems to add some env variable that 
            //builder.AddEnvironmentVariables();
            builder.AddEnvJson(optional: false);
            
            var cfg = builder.Build();

            //cfg.ExtractConnectionStrings(ConfigurationManager.ConnectionStrings);
            var traversed = cfg.AsDictionaryPlain();
            traversed.ShouldNotBeNull();
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(traversed);
            Console.WriteLine(serialized);
        }
    }
    }
