using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.Pipelines.Core;

namespace Threax.DockerBuild.Controller
{
    class SetSecretController : IController
    {
        private readonly DeploymentConfig deploymentConfig;
        private readonly IArgsProvider argsProvider;
        private readonly IOSHandler osHandler;
        private readonly ILogger<SetSecretController> logger;

        public SetSecretController(
            DeploymentConfig deploymentConfig, 
            IArgsProvider argsProvider,
            IOSHandler osHandler, 
            ILogger<SetSecretController> logger)
        {
            this.deploymentConfig = deploymentConfig;
            this.argsProvider = argsProvider;
            this.osHandler = osHandler;
            this.logger = logger;
        }

        public Task Run()
        {
            var args = argsProvider.Args;
            if(args.Length < 4)
            {
                throw new InvalidOperationException("You must provide a secret name and source file path. (setsecret config.json secret-name /path/to/source/file)");
            }
            var name = args[2];
            var source = args[3];

            if (!File.Exists(source))
            {
                throw new InvalidOperationException($"Cannot find source file '{source}'.");
            }

            var path = deploymentConfig.GetSecretDataPath(name);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(source, path, true);
            osHandler.SetPermissions(path, deploymentConfig.User, deploymentConfig.Group);

            logger.LogInformation($"Added secret '{name}' to '{dir}' from '{source}'.");

            return Task.CompletedTask;
        }
    }
}
