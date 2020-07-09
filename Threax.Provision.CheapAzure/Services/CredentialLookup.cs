﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.Services
{
    class CredentialLookup : ICredentialLookup
    {
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IStringGenerator stringGenerator;

        public CredentialLookup(IKeyVaultManager keyVaultManager, IStringGenerator stringGenerator)
        {
            this.keyVaultManager = keyVaultManager;
            this.stringGenerator = stringGenerator;
        }

        public async Task<Credential> GetOrCreateCredentials(String keyVaultName, String credBaseName, Func<String, String> fixPassword = null, Func<String, String> fixUsername = null)
        {
            var creds = await GetCredentials(keyVaultName, credBaseName);

            if (creds.User == null)
            {
                creds.Created = true;
                creds.User = stringGenerator.CreateBase64String(24);
                if(fixUsername != null)
                {
                    creds.User = fixUsername.Invoke(creds.User);
                }
                await this.keyVaultManager.SetSecret(keyVaultName, creds.UserKey, creds.User);
            }

            if (creds.Pass == null)
            {
                creds.Created = true;
                creds.Pass = stringGenerator.CreateBase64String(32);
                if (fixPassword != null)
                {
                    creds.Pass = fixPassword.Invoke(creds.Pass);
                }
                await this.keyVaultManager.SetSecret(keyVaultName, creds.PassKey, creds.Pass);
            }

            return creds;
        }

        public async Task<Credential> GetCredentials(string keyVaultName, string credBaseName)
        {
            var created = false;
            var userKey = $"{credBaseName}-user";
            var passKey = $"{credBaseName}-pass";

            var user = await this.keyVaultManager.GetSecret(keyVaultName, userKey);
            var pass = await this.keyVaultManager.GetSecret(keyVaultName, passKey);

            return new Credential()
            {
                User = user,
                Pass = pass,
                Created = created,
                UserKey = userKey,
                PassKey = passKey
            };
        }
    }
}