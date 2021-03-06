﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;
using Threax.Provision.CheapAzure.Resources;

namespace Threax.Provision.CheapAzure.Controller.Build
{
    class BuildCompute : IResourceProcessor<Compute>
    {
        private readonly BuildConfig buildConfig;
        private readonly ILogger<BuildCompute> logger;
        private readonly IProcessRunner processRunner;

        public BuildCompute(BuildConfig buildConfig, ILogger<BuildCompute> logger, IProcessRunner processRunner)
        {
            this.buildConfig = buildConfig;
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public Task Execute(Compute resource)
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

            args += " --progress=plain";

            var startInfo = new ProcessStartInfo("docker", args);
            startInfo.Environment["DOCKER_BUILDKIT"] = "1";

            var exitCode = processRunner.RunProcessWithOutput(startInfo);
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the docker build.");
            }

            return Task.CompletedTask;
        }
    }
}
