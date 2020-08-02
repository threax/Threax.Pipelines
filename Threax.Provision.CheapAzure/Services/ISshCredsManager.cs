using System.Threading.Tasks;

namespace Threax.Provision.CheapAzure.Services
{
    interface ISshCredsManager
    {
        string PrivateKeySecretName { get; }
        string PublicKeySecretName { get; }

        Task CopySshFile(string file, string dest);
        void Dispose();
        Task<string> LoadPublicKey();
        Task<int> RunSshCommand(string command);
        Task SaveSshKnownHostsSecret();
    }
}