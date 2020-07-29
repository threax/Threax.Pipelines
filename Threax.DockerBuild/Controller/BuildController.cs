using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;

namespace Threax.K8sDeploy.Controller
{
    class BuildController : IController
    {
        private BuildConfig appConfig;
        private ILogger logger;
        private IProcessRunner processRunner;

        public BuildController(BuildConfig appConfig, ILogger<BuildController> logger, IProcessRunner processRunner)
        {
            this.appConfig = appConfig;
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public Task Run()
        {
            var context = appConfig.GetContext();
            var dockerFile = Path.GetFullPath(Path.Combine(context, appConfig.Dockerfile ?? throw new InvalidOperationException($"Please provide {nameof(appConfig.Dockerfile)} when using build.")));
            var image = appConfig.ImageName;
            var buildTag = appConfig.GetBuildTag();
            var currentTag = appConfig.GetCurrentTag();

            var args = $"build {context} -f {dockerFile} -t {image}:{buildTag} -t {image}:{currentTag}";

            if (appConfig.AlwaysPull)
            {
                args += " --pull";
            }

            var exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", args));
            if(exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the docker build.");
            }

            return Task.CompletedTask;
        }
    }
}
