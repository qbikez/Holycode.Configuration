Common.Configuration
====================

Installation
------------

    > Install-Package Common.Configuration

Concept
-------

The concept is based on [asp.net Core Configuration model](https://docs.asp.net/en/latest/fundamentals/configuration.html).

Basic Usage
-----------

Create IConfiguration:

    // create configBuilder
    var builder = ConfigFactory.CreateConfigSource(applicationBasePath);
    
    // add global env.json support
    builder.AddEnvJson(applicationBasePath);
    
    // add application-specific config
    builder.AddJsonFile("config.json", optional: true);
    builder.AddJsonFile($"config.{builder.EnvironmentName()}.json", optional: true)

    // build the configuration, so it's ready to use
    IConfiguration config = builder.Build();


Sample `config.json` file:

    {
        "services": { 
            "myapi": "http://my.org/api/v1"
        }
    }

Get values from config:

    var url = config.Get("services:myapi"); 


Environments
------------

Again, this is a concept introduced in [Asp.Net Core Multiple Environments](https://docs.asp.net/en/latest/fundamentals/environments.html)

Current environment name is determined by `ASPNET_ENV` variable. By default this is taken from an environment variable.

env.json
----------

`env.json` is a global configuration file that is intended to be shared by different projects. This line:

    builder.AddEnvJson(applicationBasePath);

Causes the configuration system to look for `env.json` file. The search algorithm is following:

1. Search for `env.json` file, by starting in `applicationBasePath` and going up the directory tree, until the file is found, or disk root is reached. 
2. If `env.json` is found, Parse it.
3. Look for `env.{builder.EnvironmentName()}.json` in the same directory as `env.json`. Parse it.
4. Look for `env.local.json` in the same dir. Parse it.

`env.json` should be put somewhere high in the directory hierarchy so it can be shared by all projects underneath. 

In most cases, `env.json` will contain configuration that is common to all environment. It can also be empty or contain only current environment name, like this:

    {
        "ASPNET_ENV": "development"
    }
  
Environment-specific configuration should stored in `env.{builder.EnvironmentName()}.json` files (e.g. `env.development.json`). 

`env.local.json` is intended to be the local configuration (e.g. current developer's settings and overrides) that should not be shared.  

Sample directory structure
--------------------------

Let's say our repository looks like this:

    repo.git/
     \- src/
         \- service1
         \- service2

We could add the following files:

    repo.git/
     \- env.json
     \- env.development.json
     \- env.local.json
     \- src/
         \- project1/
             \- config.json
         \- project2/
             \- config.json
             \- config.development.json

`env.*.json` files will contain configuration keys that are common to projects1 and project2 (e.g. connection strings). 
`config.*.json` files will contain configuration that is specific to a given project.

We can switch between environments by changing `ASPNET_ENV` variable (e.g. in `env.json` file). 

Connection strings
------------------

There are some extension methods to deal with connection strings:

    Configuration.GetConnectionStringValue(connectionStringName)

Connection strings are assumed to be stored in `connectionStrings` key:

    { 
        "connectionStrings": {  
            "MyDb": {
                "connectionString": "Data Source=my-sql-server;Initial Catalog=mydatabase1;Integrated Security=true;MultipleActiveResultSets=True",
                "provider": "System.Data.SqlClient"
            },
            "elastic-search": "searchserver:9200"
        }
    }

Integrating with System.Configuration
-------------------------------------

If you want to start using json configs in old asp.net project that uses `System.Configuration` all over the place, we can make it a little easier for you.

### Extracting connection strings to System.Configuration

In most cases, default `DbContext` constructor will search for it's connection string in `<connectionStrings>` section of `app/web.config`. To make it use connection strings from json config, call this:

    config.ExtractConnectionStrings(ConfigurationManager.ConnectionStrings);

This will extract all connection strings from `IConfiguration` object and inject them into `System.Configuration`. This way, you can remove all connection strings from `app/web.config` without modifications to rest of the codebase.

### Adding app/web.config values

If you want to use the new configuration model, but don't want to convert old `app/web.config` files, you can add all `<appSettings>` keys into `IConfiguration`:

    config.AddWebConfig();

If you want to do the other way around - fill `AppSettings` with values from `IConfiguration` object, call this:

    System.Configuration.ConfigurationManager.AppSettings.Fill(config); 