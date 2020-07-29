using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;
using Threax.Pipelines.Docker;

namespace Threax.DockerBuild.Controller
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
            int exitCode;
            String taggedImageName;

            if (!String.IsNullOrEmpty(deploymentConfig.ImageName))
            {
                taggedImageName = deploymentConfig.ImageName;
                exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"pull {taggedImageName}"));
                if (exitCode != 0)
                {
                    throw new InvalidOperationException("An error occured during the docker pull.");
                }
            }
            else
            {
                var image = buildConfig.ImageName;
                var currentTag = buildConfig.GetCurrentTag();
                taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);
            }

            exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"rm {deploymentConfig.Name} --force"));
            //It is ok if this fails, probably means it wasn't running

            var args = new StringBuilder($"--network appnet --name {deploymentConfig.Name} ");
            if (!String.IsNullOrEmpty(deploymentConfig.User) && !String.IsNullOrEmpty(deploymentConfig.Group))
            {
                args.Append($"--user {deploymentConfig.User}:{deploymentConfig.Group} ");
            }
            else
            {
                logger.LogWarning("No user and group defined. Container will run as root.");
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
                    EnsureDirectory(path, vol.Value.Type);
                    if (vol.Value.ManagePermissions)
                    {
                        osHandler.SetPermissions(path, deploymentConfig.User, deploymentConfig.Group);
                    }
                    args.Append($"-v \"{path}:{vol.Value.Destination}\" ");
                }
            }

            if (deploymentConfig.Secrets != null)
            {
                foreach (var vol in deploymentConfig.Secrets)
                {
                    var path = deploymentConfig.GetSecretDataPath(vol.Value.SecretName);

                    EnsureDirectory(path, vol.Value.Type);

                    if (!String.IsNullOrEmpty(vol.Value.Source))
                    {
                        File.Copy(vol.Value.Source, path, true);
                    }

                    osHandler.SetPermissions(path, deploymentConfig.User, deploymentConfig.Group);

                    args.Append($"-v \"{path}:{vol.Value.Destination}\" ");
                }
            }

            if (deploymentConfig.AutoMountAppSettings)
            {
                var path = deploymentConfig.GetSecretDataPath("threax-docker-appsettings-json");
                EnsureDirectory(path, PathType.File);
                var configText = configFileProvider.GetConfigText();
                File.WriteAllText(path, configText);
                osHandler.SetPermissions(path, deploymentConfig.User, deploymentConfig.Group);
                args.Append($"-v \"{path}:{deploymentConfig.AppSettingsMountPath}\" ");
            }

            if(deploymentConfig.Ports != null)
            {
                foreach(var port in deploymentConfig.Ports)
                {
                    args.Append($"-p {port} ");
                }
            }

            if (!String.IsNullOrEmpty(deploymentConfig.MemoryLimit))
            {
                args.Append($"--memory={deploymentConfig.MemoryLimit} ");
            }

            args.Append(taggedImageName);

            if (!String.IsNullOrEmpty(deploymentConfig.InitCommand))
            {
                var entryPoint = deploymentConfig.InitCommand.Split(null).First();
                var cmd = "";
                if(entryPoint.Length < deploymentConfig.InitCommand.Length)
                {
                    cmd = deploymentConfig.InitCommand.Substring(entryPoint.Length);
                }

                var initArgs = $"run -it --rm --entrypoint {entryPoint} {args}{cmd}";
                logger.LogInformation(initArgs);
                exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", initArgs));
                if (exitCode != 0)
                {
                    throw new InvalidOperationException("An error occured during docker run.");
                }
            }

            var runArgs = $"run -d --restart unless-stopped {args.ToString()}";
            logger.LogInformation(runArgs);
            exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", runArgs));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during docker run.");
            }

            return Task.CompletedTask;
        }

        private void EnsureDirectory(string path, PathType type)
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
        }
    }
}
