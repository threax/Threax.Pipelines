using System;
using System.Threading.Tasks;

namespace Threax.DockerTools.Tasks
{
    interface ILoadTask
    {
        /// <summary>
        /// Load the item into the destination's load directory. This will return the path to the file that was loaded.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        Task<String> LoadItem(String type, String dest, String source, Func<String> secretName);
    }
}