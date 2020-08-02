using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Threax.Azure.Abstractions;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.Extensions.Configuration.SchemaBinder;
using Threax.Pipelines.Core;
using Threax.Provision.CheapAzure.HiddenResources;
using Threax.Provision.CheapAzure.Resources;
using Threax.Provision.CheapAzure.Services;
using Threax.Provision.Processing;

namespace Threax.Provision.CheapAzure
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            var command = args[0];
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(args[1]));
            String jsonConfigPath = "unknown.json";

            return AppHost
            .Setup(services =>
            {
                services.AddSingleton<Config>(config);
                services.AddSingleton<IArgsProvider>(s => new ArgsProvider(args));

                var assembly = Assembly.GetEntryAssembly();
                var controllerNs = $"{typeof(Program).Namespace}.Controller.{command}";
                var controllerTypes = assembly.GetTypes().Where(i => i.Namespace == controllerNs && typeof(Threax.Provision.Hidden.IResourceProcessor).IsAssignableFrom(i)).ToList();

                foreach (var type in controllerTypes)
                {
                    services.AddScoped(type);
                }

                services.AddThreaxProvision(o =>
                {
                    var resourceNamespace = $"{typeof(Program).Namespace}.Resources";
                    var resourceTypes = assembly.GetTypes().Where(i => i.Namespace == resourceNamespace).Select(i => new KeyValuePair<string, Type>(i.Name, i));
                    o.TypeMap = new Dictionary<string, Type>(resourceTypes);
                    o.SetupResolver = r =>
                    {
                        r.AddResourceProcessors(controllerTypes);
                    };
                });

                services.AddScoped<SchemaConfigurationBinder>(s =>
                {
                    var configBuilder = new ConfigurationBuilder();
                    configBuilder.AddJsonFile(jsonConfigPath);
                    return new SchemaConfigurationBinder(configBuilder.Build());
                });

                services.AddScoped<BuildConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var buildConfig = new BuildConfig(jsonConfigPath);
                    config.Bind("Build", buildConfig);
                    buildConfig.Validate();
                    return buildConfig;
                });

                services.AddScoped<DeploymentConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var deployConfig = new DeploymentConfig(jsonConfigPath);
                    config.Bind("Deploy", deployConfig);
                    deployConfig.Validate();
                    return deployConfig;
                });

                services.AddScoped<AzureKeyVaultConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var parsed = new AzureKeyVaultConfig();
                    config.Bind("KeyVault", parsed);
                    return parsed;
                });

                services.AddScoped<AzureStorageConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var parsed = new AzureStorageConfig();
                    config.Bind("Storage", parsed);
                    return parsed;
                });

                services.AddThreaxProvisionAzPowershell(o =>
                {
                    o.UseDummyKeyVaultAccessManager = !config.UnlockCurrentUserInKeyVaults;
                });

                services.AddHttpClient();
                services.AddLogging(o =>
                {
                    o.AddConsole();
                });

                services.AddScoped<IStringGenerator, StringGenerator>();
                services.AddScoped<ICredentialLookup, CredentialLookup>();
                services.AddScoped<IVmCommands, VmCommands>();
                services.AddScoped<ISshCredsManager, SshCredsManager>();
                services.AddThreaxPipelines(o =>
                {
                    o.SetupConfigFileProvider = s => new ConfigFileProvider(jsonConfigPath);
                });
                services.AddThreaxPipelinesDocker();

                services.AddHttpClient<IMachineIpManager, MachineIpManager>();
            })
            .Run(async scope =>
            {
                var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var assembly = typeof(Program).Assembly.GetName();
                log.LogInformation($"{assembly.Name} {assembly.Version}");

                var loader = scope.ServiceProvider.GetRequiredService<IResourceDefinitionLoader>();
                ResourceDefinition definition;
                switch (command?.ToLowerInvariant())
                {
                    case "createcommon":
                        definition = new ResourceDefinition();
                        definition.Resources.Add(new Compute());
                        definition.Resources.Add(new KeyVault());
                        definition.Resources.Add(new ResourceGroup(config.ResourceGroup));
                        definition.Resources.Add(new SqlDatabase());
                        break;
                    default:
                        jsonConfigPath = Path.GetFullPath(args[2]);
                        definition = loader.LoadFromFile(jsonConfigPath);
                        definition.Resources.Add(new KeyVault());
                        break;
                }
                definition.SortResources();
                var provisioner = scope.ServiceProvider.GetRequiredService<IProvisioner>();
                await provisioner.ProcessResources(definition, scope);
            });
        }
    }
}
