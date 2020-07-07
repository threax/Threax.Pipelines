using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision;
using Threax.Provision.Processing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiExtensions
    {
        public static void AddThreaxProvision(this IServiceCollection services, Action<ProvisionConfig> configure)
        {
            var options = new ProvisionConfig();
            configure?.Invoke(options);

            services.TryAddSingleton<ProvisionConfig>(options);
            services.TryAddSingleton<IProvisionJsonSerializerProvider, ProvisionJsonSerializerProvider>();

            services.TryAddScoped<IProvisioner, Provisioner>();
            services.TryAddScoped<IResourceDefinitionLoader, ResourceDefinitionLoader>();
            services.TryAddScoped<IResourceProcessorResolver>(s =>
            {
                var resolver = new ResourceProcessorResolver();
                options.SetupResolver?.Invoke(resolver);
                return resolver;
            });
        }
    }
}
