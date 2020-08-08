using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Tasks
{
    class LoadTask : ILoadTask
    {
        private const string FileType = "file";
        private const string SecretType = "secret";
        private readonly IOSHandler osHandler;
        private readonly DeploymentConfig deploymentConfig;

        public LoadTask(IOSHandler osHandler, DeploymentConfig deploymentConfig)
        {
            this.osHandler = osHandler;
            this.deploymentConfig = deploymentConfig;
        }

        public Task<String> LoadItem(String type, String dest, String source, Func<String> secretName)
        {
            var loadDir = Path.GetFullPath(deploymentConfig.GetAppDataPath("load"));
            dest = Path.GetFullPath(Path.Combine(loadDir, dest));
            if (!dest.StartsWith(loadDir))
            {
                throw new InvalidOperationException($"The destination dir '{dest}' is not inside this app's load dir '{loadDir}'. No changes will be made.");
            }

            var destDir = Path.GetDirectoryName(dest);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            switch (type.ToLowerInvariant())
            {
                case FileType:
                    LoadFile(source, dest);
                    break;
                case SecretType:
                    LoadSecret(source, dest, secretName != null ? secretName.Invoke() : throw new InvalidOperationException($"A callback to get the '{nameof(secretName)}' is required when the type is {SecretType}."));
                    break;
                default:
                    throw new InvalidOperationException($"Type '{type}' not supported.");
            }

            osHandler.SetPermissions(dest, deploymentConfig.User, deploymentConfig.Group);

            return Task.FromResult(dest);
        }

        private void LoadFile(string source, string dest)
        {
            source = Path.GetFullPath(source);
            if (!File.Exists(source))
            {
                throw new InvalidOperationException($"The source file '{source}' does not exist.");
            }

            if (source != dest)
            {
                File.Copy(source, dest);
            }
        }

        private void LoadSecret(string sourceConfig, string dest, string secretName)
        {
            var sourceConfigDir = Path.GetDirectoryName(sourceConfig);
            if (!Directory.Exists(sourceConfigDir))
            {
                throw new InvalidOperationException($"Cannot find source config directory '{sourceConfigDir}' for file '{sourceConfig}'. This directory needs to exist and be readable.");
            }

            var sourceAppConfig = new DeploymentConfig(sourceConfig);
            var secretPath = sourceAppConfig.GetSecretDataPath(secretName);
            if (!File.Exists(secretPath))
            {
                throw new InvalidOperationException($"The secret file '{secretPath}' does not exist in the souce app.");
            }

            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            File.Copy(secretPath, dest);
        }
    }
}
