namespace SampleProvisioner.Services
{
    interface IStringGenerator
    {
        string CreateBase64String(int numBytes);
        void Dispose();
    }
}