using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Threax.Pipelines.Core;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.Services
{
    class SshCredsManager : IDisposable, ISshCredsManager
    {
        private readonly Config config;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly ICredentialLookup credentialLookup;
        private readonly IProcessRunner processRunner;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly IVmManager vmManager;
        private readonly IAppFolderFinder appFolderFinder;
        private readonly ILogger<SshCredsManager> logger;
        private String publicKeyFile;
        private String privateKeyFile;
        private String vmUser;
        private String sshHost;

        public SshCredsManager(Config config, 
            IKeyVaultManager keyVaultManager, 
            ICredentialLookup credentialLookup,
            IProcessRunner processRunner, 
            IKeyVaultAccessManager keyVaultAccessManager,
            IVmManager vmManager,
            IAppFolderFinder appFolderFinder,
            ILogger<SshCredsManager> logger)
        {
            this.config = config;
            this.keyVaultManager = keyVaultManager;
            this.credentialLookup = credentialLookup;
            this.processRunner = processRunner;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.vmManager = vmManager;
            this.appFolderFinder = appFolderFinder;
            this.logger = logger;
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(publicKeyFile))
            {
                try
                {
                    File.Delete(publicKeyFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An {ex.GetType().Name} occured cleaning up public key file '{publicKeyFile}'. Message: {ex.Message}\n{ex.StackTrace}");
                }
                try
                {
                    File.Delete(privateKeyFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An {ex.GetType().Name} occured cleaning up private key file '{privateKeyFile}'. Message: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public async Task<String> LoadPublicKey()
        {
            var publicKeyName = PublicKeySecretName;
            var publicKey = await keyVaultManager.GetSecret(config.InfraKeyVaultName, publicKeyName);
            if (publicKey == null)
            {
                throw new InvalidOperationException($"You must create a key pair with \"ssh-keygen -t rsa -b 2048 -f newazurevm\" and save it as '{publicKeyName}' and '{PrivateKeySecretName}' in the '{config.InfraKeyVaultName}' key vault. Also replace all the newlines with '**lf**'. This is needed to preserve them when they are reloaded. Then run this program again. There is no automation for this step at this time.");
            }

            return publicKey;
        }

        public async Task<int> RunSshCommand(String command)
        {
            await EnsureSshHost();

            var privateKeyPath = await LoadKeysAndGetSshPrivateKeyPath();
            var startInfo = new ProcessStartInfo("ssh", $"-i \"{privateKeyPath}\" -t \"{vmUser}@{sshHost}\" \"{command}\"");
            return processRunner.RunProcessWithOutput(startInfo);
        }

        public async Task CopySshFile(String file, String dest)
        {
            await EnsureSshHost();

            var privateKeyPath = await LoadKeysAndGetSshPrivateKeyPath();
            var startInfo = new ProcessStartInfo("scp", $"-i \"{privateKeyPath}\" \"{file}\" \"{vmUser}@{sshHost}:{dest}\"");
            var exitCode = processRunner.RunProcessWithOutput(startInfo);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Error running command scp for '{file}' to '{dest}'. Exit code was '{exitCode}'.");
            }
        }

        public String PublicKeySecretName => $"{config.VmAdminBaseKey}-ssh-public-key";

        public String PrivateKeySecretName => $"{config.VmAdminBaseKey}-ssh-private-key";

        

        private async Task<String> LoadKeysAndGetSshPrivateKeyPath()
        {
            if (privateKeyFile == null)
            {
                publicKeyFile = Path.Combine(appFolderFinder.AppUserFolder, "azure-ssh.pub");
                if (!Directory.Exists(Path.GetDirectoryName(publicKeyFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(publicKeyFile));
                }
                
                privateKeyFile = Path.Combine(appFolderFinder.AppUserFolder, "azure-ssh");
                if (!Directory.Exists(Path.GetDirectoryName(privateKeyFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(privateKeyFile));
                }

                await keyVaultAccessManager.Unlock(config.InfraKeyVaultName, config.UserId);

                var publicKey = await keyVaultManager.GetSecret(config.InfraKeyVaultName, PublicKeySecretName);
                var privateKey = await keyVaultManager.GetSecret(config.InfraKeyVaultName, PrivateKeySecretName);
                privateKey = privateKey.Replace("**lf**", "\n"); //Replace all placeholder line feeds with real line feeds

                File.WriteAllText(publicKeyFile, publicKey);
                File.WriteAllText(privateKeyFile, privateKey);

                var creds = await credentialLookup.GetCredentials(config.InfraKeyVaultName, config.VmAdminBaseKey);
                vmUser = creds.User;
            }

            return privateKeyFile;
        }

        private async Task EnsureSshHost()
        {
            if(sshHost == null)
            {
                sshHost = config.VmIpAddress ?? await vmManager.GetPublicIp(config.PublicIpName);
            }
        }
    }
}
