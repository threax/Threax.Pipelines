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

        public VmCommands(Config config, IVmManager vmManager, ISshCredsManager sshCredsManager)
        {
            this.config = config;
            this.vmManager = vmManager;
            this.sshCredsManager = sshCredsManager;
        }

        private String GetBasePath()
        {
            return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Services");
        }

        public async Task WriteFileContent(String file, String content)
        {
            var scriptPath = Path.Combine(GetBasePath(), "WriteFileContent.sh");
            await vmManager.RunCommand(config.VmName, config.ResourceGroup, "RunShellScript", scriptPath, new Hashtable { { "file", file }, { "content", Escape(content) } });
        }

        public async Task ThreaxDockerToolsRun(String file, String content)
        {
            var scriptPath = Path.Combine(GetBasePath(), "ThreaxDockerToolsRun.sh");
            await vmManager.RunCommand(config.VmName, config.ResourceGroup, "RunShellScript", scriptPath, new Hashtable { { "file", file }, { "content", Escape(content) } });
        }

        public async Task ThreaxDockerToolsExec(String file, String command, params String[] args)
        {
            var scriptPath = Path.Combine(GetBasePath(), "ThreaxDockerToolsExec.sh");
            var hashTable = new Hashtable {
                { "file", file },
                { "command", command }
            };

            if(args.Length > 4)
            {
                throw new InvalidOperationException("Only up to 5 additional arguments are supported for exec calls.");
            }

            for (var i = 0; i < args.Length; ++i)
            {
                hashTable[$"arg{i}"] = args[i];
            }

            await vmManager.RunCommand(config.VmName, config.ResourceGroup, "RunShellScript", scriptPath, hashTable);
        }

        public async Task RunSetupScript(String vmName, String resourceGroup, String acrHost, AcrCredential acrCreds)
        {
            var scriptPath = Path.Combine(GetBasePath(), "UbuntuSetup.sh");
            await vmManager.RunCommand(vmName, resourceGroup, "RunShellScript", scriptPath, new Hashtable { { "acrHost", Escape(acrHost) }, { "acrUser", Escape(acrCreds.Username) }, { "acrPass", Escape(acrCreds.Password) } });
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
