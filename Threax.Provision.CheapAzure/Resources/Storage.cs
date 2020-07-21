using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Resources
{
    public class Storage
    {
        /// <summary>
        /// The name of the storage account.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The name of the secret to create with the credentials to the storage account inside.
        /// </summary>
        public String AccessCredsSecretName { get; set; }
    }
}
