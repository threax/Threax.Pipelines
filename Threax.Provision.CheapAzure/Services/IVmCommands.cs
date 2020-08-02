using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.Services
{
    interface IVmCommands
    {
        /// <summary>
        /// Write a file to the server and then run it with `Threax.DockerTools run /file`.
        /// </summary>
        /// <param name="file">The destination file. Should be a json settings file for Threax.DockerTools.</param>
        /// <param name="content">The content of the settings file to write out.</param>
        /// <returns></returns>
        Task ThreaxDockerToolsRun(String file, String content, String user);

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
        /// Set a secret from string content in memory.
        /// </summary>
        /// <param name="vmName"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="settingsFile"></param>
        /// <param name="settingsContent"></param>
        /// <param name="secrets"></param>
        /// <returns></returns>
        Task SetSecretFromString(String vmName, String resourceGroup, String settingsFile, String settingsDest, String name, String content);

        /// <summary>
        /// Set a secret from an existing file.
        /// </summary>
        /// <param name="vmName"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="settingsFile"></param>
        /// <param name="settingsDest"></param>
        /// <param name="name"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Task SetSecretFromFile(String vmName, String resourceGroup, String settingsFile, String settingsDest, String name, String source);

        /// <summary>
        /// Run an exec command with the docker tools.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task ThreaxDockerToolsExec(String file, String command, params String[] args);
    }
}