﻿using System;
using Should;
using System.Linq;
using System.IO;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace Common.Configuration.Tests
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

    }
}
