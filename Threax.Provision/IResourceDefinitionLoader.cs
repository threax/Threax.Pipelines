namespace Threax.Provision
{
    public interface IResourceDefinitionLoader
    {
        ResourceDefinition LoadFromFile(string filePath);
    }
}