using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision;
using Threax.Provision.AzPowershell;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiExtensions
    {
        public static void AddThreaxProvisionAzPowershell(this IServiceCollection services, Action<AzPowershellOptions>? configure = null)
        {
            var options = new AzPowershellOptions();
            configure?.Invoke(options);

            services.TryAddScoped<IKeyVaultManager, KeyVaultManager>();
            if (options.UseDummyKeyVaultAccessManager)
            {
                services.TryAddScoped<IKeyVaultAccessManager, DummyKeyVaultAccessManager>();
            }
            else
            {
                services.TryAddScoped<IKeyVaultAccessManager, KeyVaultAccessManager>();
            }
            services.TryAddScoped<IArmTemplateManager, ArmTemplateManager>();
            services.TryAddScoped<IAppInsightsManager, AppInsightsManager>();
            services.TryAddScoped<IVmManager, VmManager>();
            services.TryAddScoped<IServicePrincipalManager, ServicePrincipalManager>();
            services.TryAddScoped<ISqlServerFirewallRuleManager, SqlServerFirewallRuleManager>();
            services.TryAddScoped<ISqlServerManager, SqlServerManager>();
            services.TryAddScoped<IAcrManager, AcrManager>();
            services.TryAddScoped<IStorageManager, StorageManager>();
        }
    }
}