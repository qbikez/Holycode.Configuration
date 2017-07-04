using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;


namespace Holycode.Configuration.Conventions
{

    public class EnvJsonConvention : IConfigSourceConvention
    {
        internal const string EnvironmentNameKey = "ASPNET_ENV";
        internal const string DefaultEnvironment = "development";
        public string MainConfigFile = "env.json";

        public bool IsMainConfigOptional = false;
        public bool IsEnvSpecificConfigOptional = false;

        internal const string EnvConfigFoundKey = "env:config:found";
        internal const string EnvConfigPathKey = "env:config:path";

        private string _baseDir;
        private string _envName;
        public EnvJsonConvention(string appBaseDir, string environmentName = null)
        {
            _baseDir = appBaseDir;
            _envName = environmentName;
        }
        public IEnumerable<IConfigurationSource> GetConfigSources()
        {
            var configFiles = FindConfigFiles();

            return GetConfigSources(configFiles);
        }

        private IEnumerable<ConfigPathSource> FindConfigFiles()
        {
            
            var toSearch = MainConfigFile;
             if (_envName != null && IsMainConfigOptional) {                 
                 // if env.json is optional, search for env.{_envName}.json
                 var searchFiles = new List<string>();
                 foreach(var split in MainConfigFile.Split('|')) {
                    var MainConfigExt = split.Substring(split.LastIndexOf('.'));
                    var MainConfigPathNoExt = split.Substring(0, split.LastIndexOf('.'));
                    searchFiles.Add(MainConfigPathNoExt + $".{_envName}" + MainConfigExt);
                 }
                 toSearch = string.Join("|", searchFiles);
            }
            //Console.WriteLine($"searching for config files: '{toSearch}' in '{_baseDir}'. _envName: {_envName} IsMainConfigOptional: {IsMainConfigOptional}");
            var configFiles = new ConfigFileFinder().Find(_baseDir, toSearch, stopOnFirstMatch: true);
            return configFiles;
        }

        private IEnumerable<IConfigurationSource> GetConfigSources(IEnumerable<ConfigPathSource> configFiles)
        {
            var builder = new ConfigurationBuilder();
            foreach (var file in configFiles)
            {
                var path = file.Source;
                var dir = Path.GetDirectoryName(path);
                var MainConfigNameNoExt = Path.GetFileNameWithoutExtension(path);
                
                if (MainConfigNameNoExt.EndsWith(".default")) MainConfigNameNoExt = MainConfigNameNoExt.Substring(0, MainConfigNameNoExt.Length - ".default".Length);                
                if (MainConfigNameNoExt.EndsWith($".{_envName}")) MainConfigNameNoExt = MainConfigNameNoExt.Substring(0, MainConfigNameNoExt.Length - $".{_envName}".Length);
                

                try
                {
                    builder.AddEnvironmentVariables();

                    AddDefaultFiles(dir, MainConfigNameNoExt, builder);

                    AddMainFile(path, builder, IsMainConfigOptional);

                    var env = GetEnvName(builder);
                    builder.Set(EnvironmentNameKey, env);
                    AddEnvFiles(dir, MainConfigNameNoExt, env, builder, IsEnvSpecificConfigOptional);
                    AddOverrideFiles(dir, MainConfigNameNoExt, env, builder);
                }
                catch (Exception ex)
                {
                    throw new FileLoadException($"Failed to load config file {path}: {ex.Message}", ex);
                }
            }

            return builder.Sources;
        }

        private string GetEnvName(ConfigurationBuilder builder)
        {
            var env = _envName;
            if (string.IsNullOrWhiteSpace(env)) env = builder.Get(EnvironmentNameKey);
            if (string.IsNullOrWhiteSpace(env)) env = DefaultEnvironment;

            return env;
        }

        private void AddMainFile(string path, ConfigurationBuilder builder, bool optional)
        {
            // if MainConfigFile contains folder, it will be already included in dir. strip it from mainconfigfile
            if (!File.Exists(path) && !optional) throw new FileLoadException($"Failed to load main config file {path}", path);
            builder.AddJsonFile(path, optional: optional);

            builder.Set(EnvConfigFoundKey, "true");
            builder.Set(EnvConfigPathKey, path);
        }

        private void AddDefaultFiles(string dir, string MainConfigNameNoExt, ConfigurationBuilder builder)
        {
            builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.default.json"), optional: true);
        }

        private void AddEnvFiles(string dir, string MainConfigNameNoExt, string env, ConfigurationBuilder builder, bool optional)
        {
            builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.{env}.json"), optional: optional);
        }
        private void AddOverrideFiles(string dir, string MainConfigNameNoExt, string env, ConfigurationBuilder builder)
        {
            builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.override.json"), optional: true);
            builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.{env}.override.json"), optional: true);
        }
    }
}