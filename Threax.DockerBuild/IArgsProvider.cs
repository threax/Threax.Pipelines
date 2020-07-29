namespace Threax.DockerBuild
{
    public interface IArgsProvider
    {
        string[] Args { get; }
    }
}