using System.Threading.Tasks;

namespace Threax.Provision.CheapAzure.Services
{
    interface ISshCredsManager
    {
        string PrivateKeySecretName { get; }
        string PublicKeySecretName { get; }

        Task CopySshFile(string file, string dest);
        Task CopySshStringToFile(string content, string dest);
        void Dispose();
        Task<string> LoadPublicKey();
        Task RunSshCommand(string command, int safeExitCode = 0);
    }
}