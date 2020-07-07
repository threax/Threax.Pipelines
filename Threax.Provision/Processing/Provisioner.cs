using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision.Hidden;

namespace Threax.Provision.Processing
{
    public class Provisioner : IProvisioner
    {
        IResourceProcessorResolver resolver;
        private readonly ILogger<Provisioner> logger;

        public Provisioner(IResourceProcessorResolver resolver, ILogger<Provisioner> logger)
        {
            this.resolver = resolver;
            this.logger = logger;
        }

        public async Task ProcessResources(ResourceDefinition resourceDefinition, IServiceScope scope)
        {
            foreach (var resource in resourceDefinition.Resources)
            {
                var resourceType = resource.GetType();
                Type processorType;
                if (resolver.TryGetProcessorType(resourceType, out processorType))
                {
                    var processor = scope.ServiceProvider.GetRequiredService(processorType);
                    var executeFunc = processorType.GetMethod("Execute");
                    logger.LogInformation($"Processing Resource Type '{resourceType.Name}'.");
                    var task = (Task)executeFunc.Invoke(processor, new object[] { resource });
                    await task;
                }
                else
                {
                    logger.LogInformation($"Cannot find processor for '{resourceType.Name}'. Skipping Processing.");
                }
            }
        }
    }
}
