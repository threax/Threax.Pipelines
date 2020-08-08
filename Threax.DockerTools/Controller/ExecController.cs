using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.DockerTools.Tasks;
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
        private readonly ILoadTask loadTask;

        public ExecController(
            BuildConfig buildConfig,
            DeploymentConfig deploymentConfig,
            IArgsProvider argsProvider,
            IProcessRunner processRunner,
            ILogger<ExecController> logger,
            ILoadTask loadTask)
        {
            this.buildConfig = buildConfig;
            this.deploymentConfig = deploymentConfig;
            this.argsProvider = argsProvider;
            this.processRunner = processRunner;
            this.logger = logger;
            this.loadTask = loadTask;
        }

        public async Task Run()
        {
            var filesToRemove = new List<String>();
            try
            {
                var args = argsProvider.Args;
                var i = 1;
                var commandName = args.Length > ++i ? args[i] : throw new InvalidOperationException("You must provide a command name to run. (exec config.json command-name)");

                var command = "";
                if (deploymentConfig.Commands?.TryGetValue(commandName, out command) != true)
                {
                    throw new InvalidOperationException($"Cannot find exec command '{commandName}' in Deploy.Commands.");
                }

                String loadContainerDest = null;
                var realArgs = new List<String>();
                while (++i < args.Length)
                {
                    var currentArg = args[i];
                    //Load optional file arguments, any position can start with --exec-load to load a secret and use its path in the final argument list
                    if (currentArg == "--exec-load")
                    {
                        var type = args.Length > ++i ? args[i] : throw new InvalidOperationException($"You must include a type argument in position {i}.");
                        var dest = args.Length > ++i ? args[i] : throw new InvalidOperationException($"You must include a destination argument in position {i}.");
                        var source = args.Length > ++i ? args[i] : throw new InvalidOperationException($"You must include a source argument in position {i}.");

                        var result = await loadTask.LoadItem(type, dest, source, () => args.Length > ++i ? args[i] : throw new InvalidOperationException($"You must include a secret name argument in position {i}."));
                        filesToRemove.Add(result);
                        logger.LogInformation($"Added load file '{result}'.");

                        //Figure out in container path
                        loadContainerDest = EnsureLoadContainerDest(loadContainerDest);

                        realArgs.Add(Path.Combine(loadContainerDest, dest).Replace('\\', '/'));
                    }
                    else
                    {
                        realArgs.Add(currentArg);
                    }
                }

                try
                {
                    command = string.Format(command, realArgs.ToArray());
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException("A formatting exception occured formatting the command string. Please make sure you are providing enough arguments in your call to exec.", ex);
                }

                var containerName = deploymentConfig.Name;

                var execArgs = $"exec {containerName} {command}";

                logger.LogInformation($"Running command '{commandName}' on container '{containerName}'.");

                var exitCode = processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("docker", execArgs));
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"An error occured running the command '{commandName}' on '{containerName}'");
                }
            }
            finally
            {
                foreach (var file in filesToRemove)
                {
                    logger.LogInformation($"Removing load file '{file}'.");
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"{ex.GetType().Name} removing file {file}.{Environment.NewLine}Message: {ex.Message}");
                    }
                }
            }
        }

        private string EnsureLoadContainerDest(string loadContainerDest)
        {
            if (loadContainerDest == null)
            {
                //Look for a volume called load
                if (deploymentConfig.Volumes.TryGetValue("Load", out var vol))
                {
                    loadContainerDest = vol.Destination ?? throw new InvalidOperationException($"Your 'Load' volume must also include a '{nameof(vol.Destination)}' argument.");
                }
                else
                {
                    throw new InvalidOperationException("In order to use the --exec-load argument on exec commands your app must define a Volume named 'Load'.");
                }
                loadContainerDest = deploymentConfig.Volumes["Load"].Destination;
            }

            return loadContainerDest;
        }
    }
}
