using System.Threading.Tasks;

namespace Threax.DockerTools.Tasks
{
    interface ICreateCertificateTask
    {
        Task Execute(string commonName, int expirationMonths, string path);
    }
}