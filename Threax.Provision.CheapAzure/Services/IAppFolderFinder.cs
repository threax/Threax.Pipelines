namespace Threax.Provision.CheapAzure.Services
{
    interface IAppFolderFinder
    {
        string AppUserFolder { get; }

        string GetTempProvisionPath();
    }
}