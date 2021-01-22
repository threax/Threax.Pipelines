using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.DeployConfig
{
    /// <summary>
    /// A device definition.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// The source device.
        /// </summary>
        public String Source { get; set; }

        /// <summary>
        /// The path to mount the device in in the container. This can be null to mount the device to the same path.
        /// </summary>
        public String Destination { get; set; }
    }
}
