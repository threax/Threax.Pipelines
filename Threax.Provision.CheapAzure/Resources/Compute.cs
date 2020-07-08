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
        /// Not sure what this does, but it was in the arm template.
        /// </summary>
        public bool AlwaysOn { get; set; } = false;
    }
}
