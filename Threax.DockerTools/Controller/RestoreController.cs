using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.DockerTools.Tasks;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Controller
{
    class RestoreController : IController
    {
        private readonly DeploymentConfig deploymentConfig;
        private ILogger logger;
        private IProcessRunner processRunner;
        private readonly IRunTask runTask;
        private readonly IArgsProvider argsProvider;
        private readonly IStopContainerTask stopContainerTask;

        public RestoreController(
            DeploymentConfig deploymentConfig,
            ILogger<RunController> logger,
            IProcessRunner processRunner,
            IRunTask runTask,
            IArgsProvider argsProvider,
            IStopContainerTask stopContainerTask)
        {
            this.deploymentConfig = deploymentConfig;
            this.logger = logger;
            this.processRunner = processRunner;
            this.runTask = runTask;
            this.argsProvider = argsProvider;
            this.stopContainerTask = stopContainerTask;
        }

        public Task Run()
        {
            int exitCode;
            var args = argsProvider.Args;
            var restart = !args.Contains("norestart");

            try
            {
                //Get data path
                var destinationPath = Path.GetFullPath(deploymentConfig.AppDataBasePath);

                //Get backup archive source path
                var backupSearchPath = deploymentConfig.DeploymentBasePath;
                var userProvidedPath = deploymentConfig.BackupDataPath;
                if (!String.IsNullOrEmpty(userProvidedPath))
                {
                    backupSearchPath = Path.GetFullPath(Path.Combine(backupSearchPath, userProvidedPath));
                }

                var backupSearch = $"{deploymentConfig.Name}-*.tar.gz";
                var backupPath = Directory.EnumerateFiles(backupSearchPath, backupSearch, SearchOption.TopDirectoryOnly)
                    .OrderByDescending(i => i)
                    .FirstOrDefault();

                if(backupPath == null)
                {
                    throw new InvalidOperationException($"Cannot find any backup files of the pattern '{backupSearch}'");
                }

                if (!File.Exists(backupPath))
                {
                    throw new InvalidOperationException($"Cannot find backup file '{backupPath}'. No changes made.");
                }

                //Do restore
                logger.LogInformation($"Restoring data folder from archive '{backupPath}' to '{destinationPath}'");
                stopContainerTask.StopContainer(deploymentConfig.Name);

                if (Directory.Exists(destinationPath))
                {
                    Directory.Delete(destinationPath, true);
                }

                var destParent = Path.GetFullPath(Path.Combine(destinationPath, ".."));
                if (!Directory.Exists(destParent))
                {
                    Directory.CreateDirectory(destParent);
                }

                exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("tar", $"xpvzf {backupPath}")
                {
                    WorkingDirectory = destParent
                });

                if (exitCode != 0)
                {
                    throw new InvalidOperationException("Error running tar command.");
                }
            }
            catch (Exception ex)
            {
                if (restart)
                {
                    logger.LogError(ex, $"{ex.GetType().Name} while restoring data.{Environment.NewLine}Message: '{ex.Message}'.");
                }
                else
                {
                    throw;
                }
            }

            if (restart)
            {
                return runTask.Run();
            }
            else
            {
                logger.LogInformation($"Restored with norestart option. Deployment '{deploymentConfig.Name}' needs to be restarted manually.");
                return Task.FromResult(0);
            }
        }
    }
}
