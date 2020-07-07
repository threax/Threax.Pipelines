using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;
using Threax.Pipelines.Docker;

namespace Threax.K8sDeploy.Controller
{
    class PushController : IController
    {
        private readonly BuildConfig appConfig;
        private readonly ILogger logger;
        private readonly IImageManager imageManager;
        private readonly IProcessRunner processRunner;

        public PushController(BuildConfig appConfig, ILogger<PushController> logger, IImageManager imageManager, IProcessRunner processRunner)
        {
            this.appConfig = appConfig;
            this.logger = logger;
            this.imageManager = imageManager;
            this.processRunner = processRunner;
        }

        public Task Run()
        {
            var clonePath = appConfig.ClonePath;
            var dockerFile = Path.GetFullPath(Path.Combine(clonePath, appConfig.Dockerfile ?? throw new InvalidOperationException($"Please provide {nameof(appConfig.Dockerfile)} when using push.")));
            var image = appConfig.ImageName;
            var buildTag = appConfig.GetBuildTag();
            var currentTag = appConfig.GetCurrentTag();

            var args = $"push {image}:{buildTag} -f {dockerFile} -t {image}:{buildTag} -t {image}:{currentTag}";

            if (appConfig.AlwaysPull)
            {
                args += " --pull";
            }

            processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", args));

            return Task.CompletedTask;
        }
    }
}
