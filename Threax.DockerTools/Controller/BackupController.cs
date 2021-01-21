using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.DockerTools.Tasks;
using Threax.Pipelines.Core;
using Threax.Pipelines.Docker;

namespace Threax.DockerTools.Controller
{
    class BackupController : IController
    {
        private readonly DeploymentConfig deploymentConfig;
        private ILogger logger;
        private IProcessRunner processRunner;
        private readonly IRunTask runTask;
        private readonly IArgsProvider argsProvider;
        private readonly IStopContainerTask stopContainerTask;

        public BackupController(
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
                //Get source path
                var fullDataPath = Path.GetFullPath(deploymentConfig.AppDataBasePath);
                var dataParentPath = Path.GetFullPath(Path.Combine(fullDataPath, ".."));
                var dataFolder = Path.GetFileName(fullDataPath);

                //Get backup destination path
                var backupPath = deploymentConfig.DeploymentBasePath;
                var userProvidedPath = deploymentConfig.BackupDataPath;
                if (!String.IsNullOrEmpty(userProvidedPath))
                {
                    backupPath = Path.GetFullPath(Path.Combine(backupPath, userProvidedPath));
                }
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                backupPath = $"{backupPath}/{deploymentConfig.Name}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.tar.gz";

                //Do backup
                logger.LogInformation($"Backing up data folder '{fullDataPath}' to '{backupPath}'");
                stopContainerTask.StopContainer(deploymentConfig.Name);

                exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("tar", $"cvpzf {backupPath} {dataFolder}")
                {
                    WorkingDirectory = dataParentPath
                });

                if (exitCode != 0)
                {
                    throw new InvalidOperationException("Error running tar command. No backup performed.");
                }
            }
            catch (Exception ex)
            {
                if (restart)
                {
                    logger.LogError(ex, $"{ex.GetType().Name} while backing up data.{Environment.NewLine}Message: '{ex.Message}'.");
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
                logger.LogInformation($"Backup created with norestart option. Deployment '{deploymentConfig.Name}' needs to be restarted manually.");
                return Task.FromResult(0);
            }
        }
    }
}
