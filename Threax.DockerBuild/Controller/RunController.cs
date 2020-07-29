using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;
using Threax.Pipelines.Docker;

namespace Threax.K8sDeploy.Controller
{
    class RunController : IController
    {
        private BuildConfig buildConfig;
        private readonly DeploymentConfig deploymentConfig;
        private ILogger logger;
        private IProcessRunner processRunner;
        private readonly IImageManager imageManager;
        private readonly IOSHandler osHandler;
        private readonly IConfigFileProvider configFileProvider;

        public RunController(
            BuildConfig buildConfig,
            DeploymentConfig deploymentConfig,
            ILogger<RunController> logger,
            IProcessRunner processRunner,
            IImageManager imageManager,
            IOSHandler osHandler,
            IConfigFileProvider configFileProvider)
        {
            this.buildConfig = buildConfig;
            this.deploymentConfig = deploymentConfig;
            this.logger = logger;
            this.processRunner = processRunner;
            this.imageManager = imageManager;
            this.osHandler = osHandler;
            this.configFileProvider = configFileProvider;
        }

        public Task Run()
        {
            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);

            var exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"pull {taggedImageName}"));
            if (exitCode != 0)
            {
                //This is ok
                //throw new InvalidOperationException("An error occured during the docker pull.");
            }

            exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"rm {deploymentConfig.Name} --force"));
            if (exitCode != 0)
            {
                //This is ok
                //throw new InvalidOperationException("An error occured during the docker pull.");
            }

            var args = new StringBuilder($"run -d --restart unless-stopped --network appnet --name {deploymentConfig.Name} ");
            if (!String.IsNullOrEmpty(deploymentConfig.User) && !String.IsNullOrEmpty(deploymentConfig.Group))
            {
                args.Append($"--user {deploymentConfig.User}:{deploymentConfig.Group} ");
            }
            else
            {
                logger.LogWarning("No user and group defined. App will run as root.");
            }

            if (deploymentConfig.Environment != null)
            {
                foreach (var env in deploymentConfig.Environment)
                {
                    args.Append($"-e {env.Key}={env.Value} ");
                }
            }

            if (deploymentConfig.Volumes != null)
            {
                foreach (var vol in deploymentConfig.Volumes)
                {
                    var path = deploymentConfig.GetAppDataPath(vol.Value.Source);
                    EnsureDirectory(path, vol.Value.Type, vol.Value.ManagePermissions);
                    args.Append($"-v \"{path}:{vol.Value.Destination}\" ");
                }
            }

            if (deploymentConfig.Secrets != null)
            {
                foreach (var vol in deploymentConfig.Secrets)
                {
                    var path = deploymentConfig.GetSecretDataPath(vol.Value.SecretName);

                    EnsureDirectory(path, vol.Value.Type, true);

                    if (!String.IsNullOrEmpty(vol.Value.Source))
                    {
                        File.Copy(vol.Value.Source, path, true);
                    }

                    args.Append($"-v \"{path}:{vol.Value.Destination}\" ");
                }
            }

            if (deploymentConfig.AutoMountAppSettings)
            {
                var path = deploymentConfig.GetSecretDataPath("threax-docker-appsettings-json");
                EnsureDirectory(path, PathType.File, true);
                var configText = configFileProvider.GetConfigText();
                File.WriteAllText(path, configText);
                args.Append($"-v \"{path}:{deploymentConfig.AppSettingsMountPath}\" ");
            }

            args.Append(taggedImageName);

            exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", args.ToString()));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during docker run.");
            }

            return Task.CompletedTask;
        }

        private void EnsureDirectory(string path, PathType type, bool managePermissions)
        {
            switch (type)
            {
                case PathType.Directory:
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    break;
                case PathType.File:
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported path type '{type}'.");
            }

            if (managePermissions)
            {
                osHandler.SetPermissions(path, deploymentConfig.User, deploymentConfig.Group);
            }
        }
    }
}
