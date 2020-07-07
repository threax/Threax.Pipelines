using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Services
{
    public class Credential
    {
        public String Username { get; set; }

        public String Password { get; set; }

        /// <summary>
        /// This will be true if any of the values were created on lookup.
        /// </summary>
        public bool Created { get; set; }
    }
}
