using System;
using Should;
using System.Linq;
using System.IO;
using Xunit;
using Microsoft.Extensions.Configuration;
using Holycode.Configuration.Conventions;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration.Memory;

namespace Holycode.Configuration.Tests.dotnet
{
    public class env_json_test
    {
        [Fact]
        public void global_env_json_should_be_found()
        {

            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath("."));
            src.AddEnvJson(optional: false);
        }

        [Fact]
        public void should_find_env_json_root_folder_with_absolute_path()
        {
            var envPaths = new ConfigFileFinder().Find(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"), "env.json");

            Console.WriteLine("current dir=" + Directory.GetCurrentDirectory());
            envPaths.All((p) => { Console.WriteLine("envpath:" + p); return true; });

            envPaths.Count().ShouldBeGreaterThan(0);
            envPaths.Last().Directory.ShouldEqual(Path.GetFullPath(@"input\reporoot1"));
        }

           [Fact]
        public void should_find_env_json_root_in_config_dir()
        {
            var envPaths = new ConfigFileFinder().Find(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"), "config/env.json");

            Console.WriteLine("current dir=" + Directory.GetCurrentDirectory());
            envPaths.All((p) => { Console.WriteLine("envpath:" + p); return true; });

            envPaths.Count().ShouldBeGreaterThan(0);
            envPaths.Last().Directory.ShouldEqual(Path.GetFullPath(@"input\reporoot1\config"));
        }


        [Fact]
        public void should_find_env_json_root_folder_with_relative_path()
        {
            var envPaths = new ConfigFileFinder().Find(@"input\reporoot1\subfolder\projectfolder\", "env.json");

            Console.WriteLine("current dir=" + Directory.GetCurrentDirectory());
            envPaths.All((p) => { Console.WriteLine("envpath:" + p); return true; } );

            envPaths.Count().ShouldBeGreaterThan(0);
            envPaths.Last().Directory.ShouldEqual(@"input\reporoot1");
        }

        [Fact]
        public void should_include_env_specific_config()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            src.AddEnvJson(optional: false);

            var val = src.Get("test_devel");

            src.EnvironmentName().ShouldEqual("development");
            val.ShouldNotBeNull();
            val.ShouldEqual("test_from_devel");
        }

        [Fact]
        public void should_load_specified_env()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            src.AddEnvJson(optional: false, environment: "test");

            var val = src.Get("test_local");
            var isTest = src.Get("is_test_env");

            val.ShouldNotBeNull();
            val.ShouldEqual("test_from_local");
            isTest.ShouldNotBeNull();
            isTest.ShouldEqual("true");
        }

        [Fact]
        public void should_set_basepath_in_ctor()
        {
            var p = Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\");
            var src = ConfigFactory.CreateConfigSource(p);

            var basepath = src.AppBasePath();

            basepath.ShouldNotBeNull();
            basepath.ShouldEqual(p);
        }

        [Fact]
        public void should_include_override_config()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            src.AddEnvJson(optional: false, environment: "test");

            var val = src.Get("test_local");

            val.ShouldNotBeNull();
            val.ShouldEqual("test_from_local");
        }

        [Fact]
        public void in_mem_collection_order() {
            var src = new ConfigurationBuilder();
            
            src.AddInMemoryCollection(new Dictionary<string, string>() {
                { "key1", "val1" }
            });

            var val = src.Build().Get("key1");
            val.ShouldEqual("val1");

            src.AddInMemoryCollection(new Dictionary<string, string>() {
                { "key1", "val2" }
            });

            val = src.Build().Get("key1");
            val.ShouldEqual("val2");
        }

        [Fact]
        public void in_mem_collection_replace()
        {
            var src = new ConfigurationBuilder();

            src.AddInMemoryCollection(new Dictionary<string, string>() {
                { "key1", "val1" }
            });

            var val = src.Build().Get("key1");
            val.ShouldEqual("val1");

            var mem = src.Sources.LastOrDefault() as MemoryConfigurationSource;

            var data = mem.InitialData?.ToList() ?? new List<KeyValuePair<string, string>>();
            data.RemoveAll(d => d.Key == "key1");
            data.Add(new KeyValuePair<string, string>("key1", "val2"));
            mem.InitialData = data;

            val = src.Build().Get("key1");
            val.ShouldEqual("val2");
        }

        [Fact]
        public void set_should_override_previous_set_values()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));

            src.Set("some_key", "from code");
            var val = src.Get("some_key");
            val.ShouldEqual("from code");

            src.Set("some_key", "from code 1");
            val = src.Get("some_key");
            val.ShouldEqual("from code 1");
        }

        [Fact]
        public void set_should_override_previous_file_values()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            
            src.Set("some_key", "from code");
            var val = src.Get("some_key");
            val.ShouldEqual("from code");

            src.Set("some_key", "from code 1");
            val = src.Get("some_key");
            val.ShouldEqual("from code 1");

            src.AddEnvJson(optional: false, environment: "test");

            val  = src.Get("some_key");
            val.ShouldEqual("from env.test.json");

            src.Set("some_key", "from code 2");
            val = src.Get("some_key");
            val.ShouldEqual("from code 2");

            
        }


        [Fact]
        public void should_include_env_override_config()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            src.AddEnvJson(optional: false, environment: "development");

            var val = src.Get("test_local");
            var devVal = src.Get("test_development_local");
            var prodVal = src.Get("test_prod_local");
            var envVal = src.Get("env_override");

            val.ShouldNotBeNull();
            val.ShouldEqual("test_development_local");
            devVal.ShouldNotBeNull();
            devVal.ShouldEqual("test_development_local");
            prodVal.ShouldBeNull();

            envVal.ShouldNotBeNull();
        }
    }
}
