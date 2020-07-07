using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using Newtonsoft.Json;
using Threax.Provision.CheapAzure.HiddenResources;
using Threax.Provision.CheapAzure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.Processing;

namespace Threax.Provision.CheapAzure
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var command = "Deploy";
            var file = Path.GetFullPath("test.json");
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("coreconfig.json"));

            var services = new ServiceCollection();
            services.AddSingleton<Config>(config);

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

            services.AddThreaxProvisionAzPowershell();

            services.AddHttpClient();
            services.AddLogging(o =>
            {
                o.AddConsole();
            });

            services.AddScoped<IStringGenerator, StringGenerator>();
            services.AddScoped<ICredentialLookup, CredentialLookup>();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var loader = scope.ServiceProvider.GetRequiredService<IResourceDefinitionLoader>();
            var definition = loader.LoadFromFile(file);
            definition.Resources.Add(new ResourceGroup(config.ResourceGroup));
            definition.Resources.Add(new KeyVault());
            definition.SortResources();
            var provisioner = scope.ServiceProvider.GetRequiredService<IProvisioner>();
            await provisioner.ProcessResources(definition, scope);
        }
    }
}
