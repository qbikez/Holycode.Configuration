using System;
using Should;
using System.Linq;
using System.IO;
using Xunit;
using Microsoft.Extensions.Configuration;

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
            var envPaths = ServicePathResolver.ExtractNames(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));

            Console.WriteLine("current dir=" + Directory.GetCurrentDirectory());
            envPaths.All((p) => { Console.WriteLine("envpath:" + p); return true; });

            envPaths.Length.ShouldBeGreaterThan(0);
            envPaths.Last().Source.ShouldEqual(Path.GetFullPath(@"input\reporoot1"));
        }


        [Fact]
        public void should_find_env_json_root_folder_with_relative_path()
        {
            var envPaths = ServicePathResolver.ExtractNames(@"input\reporoot1\subfolder\projectfolder\");

            Console.WriteLine("current dir=" + Directory.GetCurrentDirectory());
            envPaths.All((p) => { Console.WriteLine("envpath:" + p); return true; } );

            envPaths.Length.ShouldBeGreaterThan(0);
            envPaths.Last().Source.ShouldEqual(@"input\reporoot1");
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
        public void should_include_local_config()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            src.AddEnvJson(optional: false, environment: "test");

            var val = src.Get("test_local");

            val.ShouldNotBeNull();
            val.ShouldEqual("test_from_local");
        }

        [Fact]
        public void should_include_env_local_config()
        {
            var src = ConfigFactory.CreateConfigSource(Path.GetFullPath(@"input\reporoot1\subfolder\projectfolder\"));
            src.AddEnvJson(optional: false, environment: "development");

            var val = src.Get("test_local");
            var devVal = src.Get("test_development_local");
            var prodVal = src.Get("test_prod_local");

            val.ShouldNotBeNull();
            val.ShouldEqual("test_development_local");
            devVal.ShouldNotBeNull();
            devVal.ShouldEqual("test_development_local");
            prodVal.ShouldBeNull();
        }
    }
}
