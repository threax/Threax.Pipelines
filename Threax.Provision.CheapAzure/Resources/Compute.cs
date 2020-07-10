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
        /// A list of dns names for the app.
        /// </summary>
        public List<String> DnsNames { get; set; }
    }
}
