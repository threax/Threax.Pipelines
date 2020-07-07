using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Threax.Pipelines.Docker;

namespace Threax.Pipelines.Core
{
    public static class DiExtensions
    {
        public static void AddThreaxPipelinesDocker(this IServiceCollection services, Action<ThreaxPipelineDockerOptions> configure = null)
        {
            var options = new ThreaxPipelineDockerOptions();
            configure?.Invoke(options);

            services.AddScoped<IImageManager, ImageManager>();
        }
    }
}
