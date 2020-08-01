using System;
using System.Threading.Tasks;

namespace Threax.Provision.CheapAzure.Services
{
    interface ICredentialLookup
    {
        /// <summary>
        /// Get a set of credentials from the key vault. If they do not exist they will be randomly created as strings.
        /// </summary>
        /// <param name="keyVaultName">The name of the key vault to lookup values from.</param>
        /// <param name="credBaseName">The base name of the user/password pair to lookup.</param>
        /// <returns></returns>
        Task<Credential> GetOrCreateCredentials(string keyVaultName, string credBaseName);

        Task<Credential> GetCredentials(string keyVaultName, string credBaseName);
    }
}