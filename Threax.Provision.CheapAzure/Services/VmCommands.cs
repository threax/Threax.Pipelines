using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.Services
{
    class VmCommands : IVmCommands
    {
        private readonly Config config;
        private readonly IVmManager vmManager;
        private readonly ISshCredsManager sshCredsManager;
        private readonly IAppFolderFinder appFolderFinder;

        public VmCommands(Config config, IVmManager vmManager, ISshCredsManager sshCredsManager, IAppFolderFinder appFolderFinder)
        {
            this.config = config;
            this.vmManager = vmManager;
            this.sshCredsManager = sshCredsManager;
            this.appFolderFinder = appFolderFinder;
        }

        private String GetBasePath()
        {
            return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Services");
        }

        public async Task ThreaxDockerToolsRun(String file, String content, String user)
        {
            var scriptPath = Path.Combine(GetBasePath(), "ThreaxDockerToolsRun.sh");
            await vmManager.RunCommand(config.VmName, config.ResourceGroup, "RunShellScript", scriptPath, new Hashtable { { "file", file }, { "content", Escape(content) }, { "user", Escape(user) } });
        }

        public async Task ThreaxDockerToolsExec(String file, String command, params String[] args)
        {
            var expanded = args.Length > 0 ? $"\"{String.Join("\", \"", args)}\"" : null;
            var exitCode = await sshCredsManager.RunSshCommand($"sudo Threax.DockerTools \"exec\" \"{file}\" \"{command}\" {expanded}");
            if (exitCode != 0)
            {
                throw new InvalidOperationException("Error running exec.");
            }
        }

        public async Task RunSetupScript(String vmName, String resourceGroup, String acrHost, AcrCredential acrCreds)
        {
            var scriptName = "UbuntuSetup.sh";
            var scriptPath = Path.Combine(GetBasePath(), scriptName);
            var scriptDest = $"~/{scriptName}";
            await sshCredsManager.CopySshFile(scriptPath, scriptDest);
            var exitCode = await sshCredsManager.RunSshCommand($"chmod 777 \"{scriptDest}\"; sudo sh \"{scriptDest}\";rm \"{scriptDest}\";");
            if(exitCode != 0)
            {
                //This won't do happen, needs real error checking.
                throw new InvalidOperationException("Error running setup script.");
            }
            //Good way, send password as file
            var passwordFile = appFolderFinder.GetTempProvisionPath();
            try
            {
                File.WriteAllText(passwordFile, acrCreds.Password);

                var destPasswordFile = "~/acrpass";
                await sshCredsManager.CopySshFile(passwordFile, destPasswordFile);
                exitCode = await sshCredsManager.RunSshCommand($"cat \"{destPasswordFile}\" | sudo docker login -u \"{acrCreds.Username}\" --password-stdin \"{acrHost}\"; rm \"{destPasswordFile}\"");
                if (exitCode != 0)
                {
                    //This won't do happen, needs real error checking.
                    throw new InvalidOperationException($"Error Logging ACR '{acrHost}'.");
                }
            }
            finally
            {
                if (File.Exists(passwordFile))
                {
                    File.Delete(passwordFile);
                }
            }

            //Bad way that exposes password
            //exitCode = await sshCredsManager.RunSshCommand($"sudo docker login -u \"{acrCreds.Username}\" -p \"{acrCreds.Password}\" \"{acrHost}\"");
            //if (exitCode != 0)
            //{
            //    //This won't do happen, needs real error checking.
            //    throw new InvalidOperationException($"Error Logging ACR '{acrHost}'.");
            //}
        }

        public async Task SetSecretFromString(String vmName, String resourceGroup, String settingsFile, String settingsDest, String name, String content)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, content);
                await SetSecretFromFile(vmName, resourceGroup, settingsFile, settingsDest, name, tempFile);
            }
            finally
            {
                //Any exceptions here are intentionally left to bubble up
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public async Task SetSecretFromFile(String vmName, String resourceGroup, String settingsFile, String settingsDest, String name, String source)
        {
            var tempPath = $"~/{Path.GetFileName(Path.GetRandomFileName())}";
            try
            {
                //Copy settings file
                var settingsFolder = Path.GetDirectoryName(settingsDest).Replace('\\', '/');
                await sshCredsManager.RunSshCommand($"sudo mkdir \"{settingsFolder}\"");
                await sshCredsManager.CopySshFile(settingsFile, tempPath);
                await sshCredsManager.RunSshCommand($"sudo mv \"{tempPath}\" \"{settingsDest}\"");

                //Copy Secret
                await sshCredsManager.CopySshFile(source, tempPath);
                var exitCode = await sshCredsManager.RunSshCommand($"sudo Threax.DockerTools SetSecret \"{settingsDest}\" \"{name}\" \"{tempPath}\"");
                if (exitCode != 0)
                {
                    throw new InvalidOperationException("Error setting secret.");
                }
            }
            finally
            {
                await sshCredsManager.RunSshCommand($"sudo rm {tempPath}");
            }
        }

        private static string Escape(string content)
        {
            var sb = new StringBuilder(content.Length * 2);
            for(var i = 0; i < content.Length; ++i)
            {
                var c = content[i];
                var next = i + 1;
                bool isCrLf = c == '\r' && next < content.Length && content[next] == '\n';
                if (!isCrLf)
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
            }
            content = sb.ToString();
            return content;
        }
    }
}
