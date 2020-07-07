namespace Threax.Provision.CheapAzure.Services
{
    interface IStringGenerator
    {
        string CreateBase64String(int numBytes);
        void Dispose();
    }
}