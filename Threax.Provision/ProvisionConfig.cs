using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision.Processing;

namespace Threax.Provision
{
    public class ProvisionConfig
    {
        /// <summary>
        /// The map of names to types to use to configure the resource types defined in json.
        /// </summary>
        public Dictionary<String, Type> TypeMap { get; set; }

        /// <summary>
        /// Implement this function to setup the resolver to find the processors you want to use.
        /// </summary>
        public Action<IResourceProcessorResolver> SetupResolver { get; set; }
    }
}
