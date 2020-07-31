using System;
using System.Collections.Generic;
using System.Security.Principal;
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
        /// The name of the app insights secret to store the instrumentation key under.
        /// </summary>
        public String AppInsightsSecretName { get; set; }
    }
}
