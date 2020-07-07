namespace Threax.Pipelines.Docker
{
    public interface IImageManager
    {
        string FindLatestImage(string image, string baseTag, string currentTag);
    }
}