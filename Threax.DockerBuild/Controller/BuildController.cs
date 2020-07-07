using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.K8sDeploy.Services;

namespace Threax.K8sDeploy.Controller
{
    class BuildController : IController
    {
        private DeploymentConfig appConfig;
        private ILogger logger;
        private IProcessRunner processRunner;

        public BuildController(DeploymentConfig appConfig, ILogger<BuildController> logger, IProcessRunner processRunner)
        {
            this.appConfig = appConfig;
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public Task Run()
        {
            var clonePath = appConfig.ClonePath;
            var dockerFile = Path.GetFullPath(Path.Combine(clonePath, appConfig.Dockerfile ?? throw new InvalidOperationException($"Please provide {nameof(appConfig.Dockerfile)} when using build.")));
            var image = appConfig.Name;
            var buildTag = appConfig.GetBuildTag();
            var currentTag = appConfig.GetCurrentTag();

            var args = $"build {clonePath} -f {dockerFile} -t {image}:{buildTag} -t {image}:{currentTag}";

            if (appConfig.AlwaysPull)
            {
                args += " --pull";
            }

            processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", args));

            return Task.CompletedTask;
        }
    }
}
