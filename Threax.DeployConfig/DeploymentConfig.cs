using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Threax.DeployConfig
{
    /// <summary>
    /// Configuration for k8s deployment.
    /// </summary>
    public class DeploymentConfig
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceFile"></param>
        public DeploymentConfig(String sourceFile)
        {
            this.SourceFile = sourceFile;
            this.DeploymentBasePath = Path.GetFullPath(Path.GetDirectoryName(this.SourceFile));
            this.AppDataBasePath = Path.GetFullPath(Path.Combine(DeploymentBasePath, "data"));
            this.SecretDataBasePath = Path.GetFullPath(Path.Combine(DeploymentBasePath, "secrets"));
        }

        /// <summary>
        /// The path to the source file of this app config.
        /// </summary>
        [JsonIgnore]
        public String SourceFile { get; private set; }

        /// <summary>
        /// The parent folder for deployment data.
        /// </summary>
        [JsonIgnore]
        public String DeploymentBasePath { get; set; }

        /// <summary>
        /// The path that provides the root volume for relative volume mounts.
        /// </summary>
        [JsonIgnore]
        public String AppDataBasePath { get; private set; }

        /// <summary>
        /// The path that provides the root volume for relative secret volume mounts. Not all consumers will use this.
        /// </summary>
        [JsonIgnore]
        public String SecretDataBasePath { get; private set; }

        /// <summary>
        /// The name of the app. Is used as a unique key for many settings including urls.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The domain to host the apps on.
        /// </summary>
        public String Domain { get; set; } = "dev.threax.com";

        /// <summary>
        /// The user id to run the app as. Default: 10000.
        /// </summary>
        public String User { get; set; } = "10000";

        /// <summary>
        /// The group id to run the app as. Default: 10000.
        /// </summary>
        public String Group { get; set; } = "10000";

        /// <summary>
        /// The base path to use when backing up data files.
        /// </summary>
        public String BackupDataPath { get; set; }

        /// <summary>
        /// If this is set to a string that command will be run inside an InitContainer before the main container is started.
        /// </summary>
        public String InitCommand { get; set; }

        /// <summary>
        /// Key vault pairs of secrets to load for the init environment.
        /// The key should be in in dot format and the value the name of the secret.
        /// e.g. "AppConfig.ConnectionString": "secret-name"
        /// </summary>
        public Dictionary<String, String> InitSecrets { get; set; }

        /// <summary>
        /// A map of volume mounts.
        /// </summary>
        public Dictionary<String, Volume> Volumes { get; set; }

        /// <summary>
        /// A map of secrets.
        /// </summary>
        public Dictionary<String, Secret> Secrets { get; set; }

        /// <summary>
        /// Key value pairs for environment variables.
        /// </summary>
        public Dictionary<String, String> Environment { get; set; }

        /// <summary>
        /// Key vaulue pairs for commands that can be run through the tools against a running instance of the app.
        /// </summary>
        public Dictionary<String, String> Commands { get; set; }

        /// <summary>
        /// The memory limit to set. Null means unlimited. This should be in the docker memory format, e.g. '400m'. Default: null
        /// </summary>
        public String MemoryLimit { get; set; }

        /// <summary>
        /// Port mappings in the format host:container
        /// </summary>
        public List<String> Ports { get; set; }

        /// <summary>
        /// The name of the pod info json file to generate. Default: pod.json.
        /// </summary>
        public String PodJsonFile { get; set; } = "pod.json";

        /// <summary>
        /// Set this to true to auto mount the app settings config. Default: true.
        /// </summary>
        public bool AutoMountAppSettings { get; set; } = true;

        /// <summary>
        /// The mount path for the appsettings file. Default: /app/appsettings.Production.json.
        /// </summary>
        public String AppSettingsMountPath { get; set; } = "/app/appsettings.Production.json";

        /// <summary>
        /// The sub path for the appsettings file. Default: appsettings.Production.json.
        /// </summary>
        public String AppSettingsSubPath { get; set; } = "appsettings.Production.json";

        /// <summary>
        /// Set this to the name of an image to force that image to be used instead of a discovered image from the local system.
        /// This image must be pullable from the target container host or an error will be thrown.
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Validate that this config is correct. Throws an exception if there is an error.
        /// </summary>
        public void Validate()
        {
            //Does nothing right now
        }

        /// <summary>
        /// Get a path in the app data path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String GetAppDataPath(String path)
        {
            return Path.GetFullPath(Path.Combine(AppDataBasePath, path));
        }

        /// <summary>
        /// Get a path in the secret folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String GetSecretDataPath(String path)
        {
            return Path.GetFullPath(Path.Combine(SecretDataBasePath, path));
        }

        /// <summary>
        /// Get a path relative to the config file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public String GetConfigPath(String path)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile), path));
        }
    }
}
