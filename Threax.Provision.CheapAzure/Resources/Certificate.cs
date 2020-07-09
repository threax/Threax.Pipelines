using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Resources
{
    class Certificate
    {
        /// <summary>
        /// The name of the cert in the key vault.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// A common name for the cert.
        /// </summary>
        public String CN { get; set; } = "dontcare";

        /// <summary>
        /// The number of months until expiration.
        /// </summary>
        public int ExpirationMonths { get; set; } = 12;
    }
}
