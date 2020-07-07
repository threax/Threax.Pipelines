using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;

namespace SampleProvisioner.Services
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

        public async Task<Credential> GetCredentials(String keyVaultName, String credBaseName, Func<String, String> fixPassword = null, Func<String, String> fixUsername = null)
        {
            var created = false;
            var userKey = $"{credBaseName}-user";
            var passKey = $"{credBaseName}-pass";

            var user = await this.keyVaultManager.GetSecret(keyVaultName, userKey);
            if (user == null)
            {
                created = true;
                user = stringGenerator.CreateBase64String(24);
                if(fixUsername != null)
                {
                    user = fixUsername.Invoke(user);
                }
                await this.keyVaultManager.SetSecret(keyVaultName, userKey, user);
            }

            var pass = await this.keyVaultManager.GetSecret(keyVaultName, passKey);
            if (pass == null)
            {
                created = true;
                pass = stringGenerator.CreateBase64String(32);
                if (fixPassword != null)
                {
                    pass = fixPassword.Invoke(pass);
                }
                await this.keyVaultManager.SetSecret(keyVaultName, passKey, pass);
            }

            return new Credential()
            {
                Username = user,
                Password = pass,
                Created = created
            };
        }
    }
}
