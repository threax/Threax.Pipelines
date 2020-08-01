﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private String publicKeyFile;
        private String privateKeyFile;
        private String vmUser;

        public SshCredsManager(Config config, IKeyVaultManager keyVaultManager, ICredentialLookup credentialLookup, IProcessRunner processRunner, IKeyVaultAccessManager keyVaultAccessManager)
        {
            this.config = config;
            this.keyVaultManager = keyVaultManager;
            this.credentialLookup = credentialLookup;
            this.processRunner = processRunner;
            this.keyVaultAccessManager = keyVaultAccessManager;
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
                throw new InvalidOperationException($"You must create a key pair with \"ssh-keygen -t rsa -b 2048 -f newazurevm\" and save it as '{publicKeyName}' and '{PrivateKeySecretName}' in the '{config.InfraKeyVaultName}' key vault. Then run this program again. There is no automation for this step at this time.");
            }

            return publicKey;
        }

        public async Task<int> RunSshCommand(String command)
        {
            if (String.IsNullOrEmpty(config.VmSshHost))
            {
                throw new InvalidOperationException($"You must include a '{nameof(config.VmSshHost)}' propety with the ip or hostname of your vm in your core configuration.");
            }

            var privateKeyPath = await LoadKeysAndGetSshPrivateKeyPath();
            var startInfo = new ProcessStartInfo("ssh", $"-i \"{privateKeyPath}\" -t \"{vmUser}@{config.VmSshHost}\" \"{command}\"");
            return processRunner.RunProcessWithOutput(startInfo);
        }

        public async Task CopySshFile(String file, String dest)
        {
            if (String.IsNullOrEmpty(config.VmSshHost))
            {
                throw new InvalidOperationException($"You must include a '{nameof(config.VmSshHost)}' propety with the ip or hostname of your vm in your core configuration.");
            }

            var privateKeyPath = await LoadKeysAndGetSshPrivateKeyPath();
            var startInfo = new ProcessStartInfo("scp", $"-i \"{privateKeyPath}\" \"{file}\" \"{vmUser}@{config.VmSshHost}:{dest}\"");
            var exitCode = processRunner.RunProcessWithOutput(startInfo);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Error running command scp for '{file}' to '{dest}'. Exit code was '{exitCode}'.");
            }
        }

        public String PublicKeySecretName => $"{config.VmAdminBaseKey}-ssh-public-key";

        public String PrivateKeySecretName => $"{config.VmAdminBaseKey}-ssh-private-key";

        private String GetUserHomePath()
        {
            //Thanks to MiffTheFox at https://stackoverflow.com/questions/1143706/getting-the-path-of-the-home-directory-in-c

            return (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }

        private async Task<String> LoadKeysAndGetSshPrivateKeyPath()
        {
            if (privateKeyFile == null)
            {
                publicKeyFile = Path.Combine(GetUserHomePath(), ".threaxprovision", "azure-ssh.pub");
                if (!Directory.Exists(Path.GetDirectoryName(publicKeyFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(publicKeyFile));
                }
                
                privateKeyFile = Path.Combine(GetUserHomePath(), ".threaxprovision", "azure-ssh");
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
    }
}
