using System;

namespace Threax.Pipelines.Core
{
    public class ThreaxPipelineCoreOptions
    {
        public Func<IServiceProvider, IConfigFileProvider> SetupConfigFileProvider { get; set; }
    }
}