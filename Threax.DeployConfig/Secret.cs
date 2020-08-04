using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.DeployConfig
{
    /// <summary>
    /// A secret definition.
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// The name of the secret to load. This is optional and one will be generated if this is not specified. Default: k8sconfig-secret-{appName}-{key}
        /// </summary>
        public String SecretName { get; set; }

        /// <summary>
        /// Use this to get the secret. Either the user defined secret name or one derived from the app name and a secret key.
        /// </summary>
        /// <param name="appName">The name of the app.</param>
        /// <param name="key">A key or unique name for the secret.</param>
        /// <returns></returns>
        public String GetSecretName(String appName, String key)
        {
            return SecretName ?? $"k8sconfig-secret-{appName.ToLowerInvariant()}-{key.ToLowerInvariant()}";
        }

        /// <summary>
        /// The source file to load a file as the secret. This is optional. If the secret already exists you can use SecretName also.
        /// </summary>
        public String Source { get; set; }

        /// <summary>
        /// The path to mount the secret in in the target container.
        /// </summary>
        public String Destination { get; set; }

        /// <summary>
        /// The type of the secret mount. Default: Directory
        /// </summary>
        public PathType Type { get; set; } = PathType.Directory;

        /// <summary>
        /// Allow the app to start even if this secret is missing. If 
        /// this is false and the file does not exist the deployment tools
        /// will stop with an error. Default: false
        /// </summary>
        public bool AllowMissing { get; set; } = false;
    }
}
