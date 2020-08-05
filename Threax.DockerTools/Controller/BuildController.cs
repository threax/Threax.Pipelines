﻿using Microsoft.Extensions.Logging;
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

            if (buildConfig.AlwaysPull)
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
