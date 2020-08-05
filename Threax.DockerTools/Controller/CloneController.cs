using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Controller
{
    class CloneController : IController
    {
        string localRefRoot = "refs/heads/";
        String remoteRefRoot = "refs/remotes/origin/";

        private BuildConfig appConfig;
        private ILogger logger;
        private readonly IProcessRunner processRunner;

        public CloneController(BuildConfig appConfig, ILogger<CloneController> logger, IProcessRunner processRunner)
        {
            this.appConfig = appConfig;
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public Task Run()
        {
            var clonePath = Path.GetFullPath(appConfig.ClonePath);
            var repo = appConfig.RepoUrl;

            if (Directory.Exists(appConfig.ClonePath))
            {
                logger.LogInformation($"Pulling changes to {clonePath}");
                var exitCode = processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("git", "pull")
                {
                    WorkingDirectory = Path.GetFullPath(appConfig.ClonePath)
                });
                if (exitCode != 0)
                {
                    throw new InvalidOperationException("Error during pull.");
                }
            }
            else
            {
                logger.LogInformation($"Cloning {repo} to {clonePath}");
                var exitCode = processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("git", $"clone \"{repo}\" \"{clonePath}\""));
                if(exitCode != 0)
                {
                    throw new InvalidOperationException("Error during clone.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
