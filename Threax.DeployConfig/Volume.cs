using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.DeployConfig
{
    /// <summary>
    /// A volume definition.
    /// </summary>
    public class Volume
    {
        /// <summary>
        /// The source directory. If no leading / is provided the path will be relative to the AppData path for the app.
        /// </summary>
        public String Source { get; set; }

        /// <summary>
        /// The path to mount the volume in in the container.
        /// </summary>
        public String Destination { get; set; }

        /// <summary>
        /// The type of the volume mount. Default: Directory
        /// </summary>
        public PathType Type { get; set; } = PathType.Directory;

        /// <summary>
        /// Set this to true to have the deploy app manage permissions for the source directory. Default: true.
        /// </summary>
        public bool ManagePermissions { get; set; } = true;
    }
}
