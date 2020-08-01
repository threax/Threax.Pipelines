using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.Model;

namespace Threax.Provision.CheapAzure.Services
{
    interface IVmCommands
    {
        /// <summary>
        /// Write a file to the server.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        Task WriteFileContent(string file, string content);

        /// <summary>
        /// Write a file to the server and then run it with `Threax.DockerTools run /file`.
        /// </summary>
        /// <param name="file">The destination file. Should be a json settings file for Threax.DockerTools.</param>
        /// <param name="content">The content of the settings file to write out.</param>
        /// <returns></returns>
        Task ThreaxDockerToolsRun(String file, String content);

        /// <summary>
        /// Run the main setup script on the server.
        /// </summary>
        /// <param name="vmName"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="acrHost"></param>
        /// <param name="acrCreds"></param>
        /// <returns></returns>
        Task RunSetupScript(String vmName, String resourceGroup, String acrHost, AcrCredential acrCreds);

        /// <summary>
        /// Set multiple secrets on the server. These are batched up to try to overcome the built in delays of doing this.
        /// </summary>
        /// <param name="vmName"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="settingsFile"></param>
        /// <param name="settingsContent"></param>
        /// <param name="secrets"></param>
        /// <returns></returns>
        Task SetSecrets(String vmName, String resourceGroup, String settingsFile, String settingsContent, IEnumerable<SetSecretModel> secrets);
    }
}