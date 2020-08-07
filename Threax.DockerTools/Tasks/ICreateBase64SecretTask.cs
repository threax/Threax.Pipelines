using System.Threading.Tasks;

namespace Threax.DockerTools.Tasks
{
    interface ICreateBase64SecretTask
    {
        Task CreateSecret(int size, string path);
    }
}