using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Threax.K8sDeployConfig
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
            this.AppDataBasePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.SourceFile), "data"));
        }

        /// <summary>
        /// The path to the source file of this app config.
        /// </summary>
        [JsonIgnore]
        public String SourceFile { get; private set; }

        /// <summary>
        /// The path that provides the root volume for relative volume mounts.
        /// </summary>
        [JsonIgnore]
        public String AppDataBasePath { get; private set; }

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
        /// If this is set to a string that command will be run inside an InitContainer before the main container is started.
        /// </summary>
        public String InitCommand { get; set; }

        /// <summary>
        /// A map of volume mounts.
        /// </summary>
        public Dictionary<String, Volume> Volumes { get; set; }

        /// <summary>
        /// A map of secrets.
        /// </summary>
        public Dictionary<String, Secret> Secrets { get; set; }

        /// <summary>
        /// The name of the pod info json file to generate. Default: pod.json.
        /// </summary>
        public String PodJsonFile { get; set; } = "pod.json";

        /// <summary>
        /// The path to the schema file when running in UpdateSchema mode.
        /// </summary>
        public String SchemaOutputPath { get; set; } = "schema.json";

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
        /// Validate that this config is correct. Throws an exception if there is an error.
        /// </summary>
        public void Validate()
        {
            if(Name == null)
            {
                throw new InvalidOperationException($"{nameof(Name)} cannot be null. Please provide a value.");
            }
        }

        public String GetAppDataPath(String path)
        {
            return Path.GetFullPath(Path.Combine(AppDataBasePath, path));
        }

        public String GetConfigPath(String path)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile), path));
        }
    }
}
