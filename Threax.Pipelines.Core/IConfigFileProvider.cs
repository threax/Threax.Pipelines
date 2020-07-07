namespace Threax.Pipelines.Core
{
    public interface IConfigFileProvider
    {
        string GetConfigText();

        T GetConfig<T>();
    }
}