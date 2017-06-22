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
        var builder = new ConfigurationBuilder();

        var toSearch = MainConfigFile;
        var MainConfigExt = MainConfigFile.Substring(MainConfigFile.LastIndexOf('.'));
        var MainConfigPathNoExt = MainConfigFile.Substring(0, MainConfigFile.LastIndexOf('.'));

        // if env.json is optional, search for env.{_envName}.json
        if (_envName != null && IsMainConfigOptional) 
            toSearch = MainConfigPathNoExt + $".{_envName}" + MainConfigExt;
        var configFiles = new ConfigFileFinder().Find(_baseDir, toSearch, stopOnFirstMatch: true);

        foreach (var file in configFiles)
        {
            var path = file.Source;
            var dir = Path.GetDirectoryName(path);
            var MainConfigNameNoExt = Path.GetFileNameWithoutExtension(MainConfigFile);
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
        builder.AddJsonFile(Path.Combine(dir,$"{MainConfigNameNoExt}.default.json"), optional: true);
    }

    private void AddEnvFiles(string dir, string MainConfigNameNoExt, string env, ConfigurationBuilder builder, bool optional) {
        builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.{env}.json"), optional: optional);
    }
    private void AddOverrideFiles(string dir,string MainConfigNameNoExt,  string env, ConfigurationBuilder builder)
    {        
        builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.override.json"), optional: true);
        builder.AddJsonFile(Path.Combine(dir, $"{MainConfigNameNoExt}.{env}.override.json"), optional: true);
    }
}
}