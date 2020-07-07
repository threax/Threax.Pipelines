using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Threax.Pipelines.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiExtensions
    {
        public static void AddThreaxPipelines(this IServiceCollection services, Action<ThreaxPipelineCoreOptions> configure = null)
        {
            var options = new ThreaxPipelineCoreOptions();
            configure?.Invoke(options);

            services.AddScoped<IProcessRunner, ProcessRunner>();
            services.AddScoped<IConfigFileProvider>(s =>
            {
                if(options.SetupConfigFileProvider == null)
                {
                    throw new InvalidOperationException($"You must provide {nameof(ThreaxPipelineCoreOptions.SetupConfigFileProvider)} to use the IConfigFileProvider.");
                }
                return options.SetupConfigFileProvider.Invoke(s);
            });

            services.AddScoped<IOSHandler>(s =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return new OSHandlerWindows();
                }
                return new OSHandlerUnix(s.GetRequiredService<IProcessRunner>());
            });
        }
    }
}
