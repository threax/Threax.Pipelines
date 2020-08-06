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

            if (buildConfig.PullImages != null)
            {
                foreach(var pullImage in buildConfig.PullImages)
                {
                    logger.LogInformation($"Pulling '{pullImage}'.");
                    processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"pull {pullImage}"));
                }
            }

            var args = $"build {context} -f {dockerFile} -t {image}:{buildTag} -t {image}:{currentTag}";

            if (buildConfig.PullAllImages)
            {
                args += " --pull";
            }

            var exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", args));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the docker build.");
            }

            return Task.CompletedTask;
        }
    }
}
