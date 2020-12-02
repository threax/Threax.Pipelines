using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Controller
{
    class BuildController : IController
    {
        private BuildConfig buildConfig;
        private ILogger logger;
        private IProcessRunner processRunner;

        public BuildController(BuildConfig buildConfig, ILogger<BuildController> logger, IProcessRunner processRunner)
        {
            this.buildConfig = buildConfig;
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public Task Run()
        {
            var context = buildConfig.GetContext();
            var dockerFile = Path.GetFullPath(Path.Combine(context, buildConfig.Dockerfile ?? throw new InvalidOperationException($"Please provide {nameof(buildConfig.Dockerfile)} when using build.")));
            var image = buildConfig.ImageName;
            var buildTag = buildConfig.GetBuildTag();
            var currentTag = buildConfig.GetCurrentTag();

            var args = $"build {context} -f {dockerFile} -t {image}:{buildTag} -t {image}:{currentTag}";

            if (buildConfig.PullAllImages)
            {
                args += " --pull";
            }

            if(buildConfig.Args != null)
            {
                foreach(var arg in buildConfig.Args)
                {
                    args += $" --build-arg {arg.Key}={arg.Value}";
                }
            }

            args += " --progress=plain";

            logger.LogInformation(args);
            var startInfo = new ProcessStartInfo("docker", args);
            startInfo.Environment["DOCKER_BUILDKIT"] = "1";

            var exitCode = processRunner.RunProcessWithOutput(startInfo);
            if(exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the docker build.");
            }

            return Task.CompletedTask;
        }
    }
}
