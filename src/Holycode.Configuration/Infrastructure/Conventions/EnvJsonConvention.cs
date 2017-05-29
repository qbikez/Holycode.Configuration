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
    public const string MainConfigFile = "env.json";

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
        bool optional = false;

        var configFiles = new UpwardFileFinder().Find(_baseDir, MainConfigFile, stopOnFirstMatch: true);

        foreach (var file in configFiles)
        {
            var path = file.Source;
            var dir = Path.GetDirectoryName(path);
            try
            {
                AddDefaultFiles(dir, builder);

                AddMainFile(dir, builder, optional);

                var env = GetEnvName(builder);
                builder.Set(EnvironmentNameKey, env);

                AddOverrideFiles(dir, env, builder, optional);
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Failed to load config file {path}", ex);
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

    private void AddMainFile(string dir, ConfigurationBuilder builder, bool optional)
    {
        var path = Path.Combine(dir, MainConfigFile);
        if (!File.Exists(path) && !optional) throw new FileLoadException($"Failed to load config file {path}", path);
        builder.AddJsonFile(path, optional: optional);
    }

    public void AddDefaultFiles(string dir, ConfigurationBuilder builder)
    {
        builder.AddJsonFile($"env.default.json", optional: true);
    }
    public void AddOverrideFiles(string dir, string env, ConfigurationBuilder builder, bool optional)
    {
        builder.AddJsonFile(Path.Combine(dir, $"env.{env}.json"), optional: optional);
        builder.AddJsonFile(Path.Combine(dir, $"env.override.json"), optional: true);
        builder.AddJsonFile(Path.Combine(dir, $"env.{env}.override.json"), optional: true);
    }
}
}