using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Pipelines.Core
{
    /// <summary>
    /// This class handles os specific trickery.
    /// </summary>
    public interface IOSHandler
    {
        /// <summary>
        /// Fix the path if needed. This is used for windows to replace c:/thedir with /c/thedir. On linux it will return the original string.
        /// </summary>
        /// <param name="path">The incoming path.</param>
        /// <returns></returns>
        string CreateDockerPath(string path);

        /// <summary>
        /// Set permissions for a user on a path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="user"></param>
        /// <param name="group"></param>
        void SetPermissions(string path, string user, string group);
    }
}
