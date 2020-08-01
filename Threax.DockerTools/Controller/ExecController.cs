using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Controller
{
    class ExecController : IController
    {
        private readonly BuildConfig buildConfig;
        private readonly DeploymentConfig deploymentConfig;
        private readonly IArgsProvider argsProvider;
        private readonly IProcessRunner processRunner;
        private readonly ILogger<ExecController> logger;

        public ExecController(
            BuildConfig buildConfig,
            DeploymentConfig deploymentConfig,
            IArgsProvider argsProvider,
            IProcessRunner processRunner,
            ILogger<ExecController> logger)
        {
            this.buildConfig = buildConfig;
            this.deploymentConfig = deploymentConfig;
            this.argsProvider = argsProvider;
            this.processRunner = processRunner;
            this.logger = logger;
        }

        public Task Run()
        {
            var args = argsProvider.Args;
            if (args.Length < 3)
            {
                throw new InvalidOperationException("You must provide a command name to run. (exec config.json command-name)");
            }
            var commandName = args[2];

            var command = "";
            if(deploymentConfig.Commands?.TryGetValue(commandName, out command) != true)
            {
                throw new InvalidOperationException($"Cannot find exec command '{commandName}' in Deploy.Commands.");
            }

            try
            {
                command = string.Format(command, args.Skip(3).ToArray());
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("A formatting exception occured formatting the command string. Please make sure you are providing enough arguments in your call to exec.", ex);
            }

            var containerName = deploymentConfig.Name;

            var execArgs = $"exec {containerName} {command}";

            logger.LogInformation($"Running command '{commandName}' on container '{containerName}'.");

            var exitCode = processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("docker", execArgs));
            if(exitCode != 0)
            {
                throw new InvalidOperationException($"An error occured running the command '{commandName}' on '{containerName}'");
            }

            return Task.CompletedTask;
        }
    }
}
