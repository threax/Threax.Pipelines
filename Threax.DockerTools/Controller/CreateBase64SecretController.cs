using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Controller
{
    class CreateBase64SecretController : IController
    {
        private readonly IArgsProvider argsProvider;
        private readonly DeploymentConfig deploymentConfig;
        private readonly IOSHandler osHandler;
        private readonly ILogger<CreateBase64SecretController> logger;

        public CreateBase64SecretController(IArgsProvider argsProvider, DeploymentConfig deploymentConfig, IOSHandler osHandler, ILogger<CreateBase64SecretController> logger)
        {
            this.argsProvider = argsProvider;
            this.deploymentConfig = deploymentConfig;
            this.osHandler = osHandler;
            this.logger = logger;
        }

        public Task Run()
        {
            if(argsProvider.Args.Length < 3)
            {
                throw new InvalidOperationException("You must provide an output file. createbase64secret /appsettings.json secret-name size");
            }

            var secretName = argsProvider.Args[2];
            int size = 32;
            if(argsProvider.Args.Length > 3)
            {
                size = int.Parse(argsProvider.Args[3]);
            }

            String base64;
            using (var numberGen = RandomNumberGenerator.Create()) //This is more portable than from services since that does not work on linux correctly
            {
                var bytes = new byte[size];
                numberGen.GetBytes(bytes);

                base64 = Convert.ToBase64String(bytes);
            }

            var path = deploymentConfig.GetSecretDataPath(secretName);
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None)))
            {
                writer.Write(base64);
            }
            osHandler.SetPermissions(path, deploymentConfig.User, deploymentConfig.Group);

            logger.LogInformation($"Added new base64 secret '{secretName}' to '{dir}'.");

            return Task.CompletedTask;
        }
    }
}
