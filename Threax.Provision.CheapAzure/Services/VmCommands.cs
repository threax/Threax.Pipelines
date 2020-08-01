using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.Model;

namespace Threax.Provision.CheapAzure.Services
{
    class VmCommands : IVmCommands
    {
        private readonly Config config;
        private readonly IVmManager vmManager;

        public VmCommands(Config config, IVmManager vmManager)
        {
            this.config = config;
            this.vmManager = vmManager;
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

        public async Task RunSetupScript(String vmName, String resourceGroup, String acrHost, AcrCredential acrCreds)
        {
            var scriptPath = Path.Combine(GetBasePath(), "UbuntuSetup.sh");
            await vmManager.RunCommand(vmName, resourceGroup, "RunShellScript", scriptPath, new Hashtable { { "acrHost", Escape(acrHost) }, { "acrUser", Escape(acrCreds.Username) }, { "acrPass", Escape(acrCreds.Password) } });
        }

        public async Task SetSecrets(String vmName, String resourceGroup, String settingsFile, String settingsContent, IEnumerable<SetSecretModel> secrets)
        {
            var scriptPath = Path.Combine(GetBasePath(), "SetSecrets.sh");
            var hashTable = new Hashtable { { "settingsFile", Escape(settingsFile) }, { "settingsContent", Escape(settingsContent) } };

            int i = 0;
            foreach(var s in secrets)
            {
                if(i > 4)
                {
                    //Can only send 5 secrets at a time, send what we have so far, ideally this won't happen the RunCommand is very slow
                    await vmManager.RunCommand(vmName, resourceGroup, "RunShellScript", scriptPath, hashTable);

                    //Reset the hash table and counter
                    hashTable = new Hashtable { { "settingsFile", Escape(settingsFile) }, { "settingsContent", Escape(settingsContent) } };
                    i = 0;
                }

                hashTable[$"file{i}"] = s.File;
                hashTable[$"name{i}"] = Escape(s.Name);
                hashTable[$"content{i}"] = Escape(s.Content);

                ++i;
            }

            //Send anything not sent above
            await vmManager.RunCommand(vmName, resourceGroup, "RunShellScript", scriptPath, hashTable);
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
