﻿using Microsoft.Extensions.Logging;
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
        private readonly BuildConfig buildConfig;
        private readonly ILogger logger;
        private readonly IImageManager imageManager;
        private readonly IProcessRunner processRunner;

        public PushController(BuildConfig buildConfig, ILogger<PushController> logger, IImageManager imageManager, IProcessRunner processRunner)
        {
            this.buildConfig = buildConfig;
            this.logger = logger;
            this.imageManager = imageManager;
            this.processRunner = processRunner;
        }

        public Task Run()
        {
            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);

            logger.LogInformation($"Pushing '{image}' with tag '{taggedImageName}'.");

            var args = $"push {taggedImageName}";

            processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", args));

            return Task.CompletedTask;
        }
    }
}