using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Resources
{
    public class Compute
    {
        /// <summary>
        /// The name of the compute.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// A command that is run before the compute is deployed.
        /// </summary>
        public String InitCommand { get; set; }

        /// <summary>
        /// Key vault pairs of secrets to load for the init environment. The key is the env variable name and the vaule is the secret to load.
        /// </summary>
        public Dictionary<String, String> InitEnv { get; set; }

        /// <summary>
        /// Not sure what this does, but it was in the arm template.
        /// </summary>
        public bool AlwaysOn { get; set; } = false;
    }
}
