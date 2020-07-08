using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Services
{
    public class Credential
    {
        public String User { get; set; }

        public String Pass { get; set; }

        /// <summary>
        /// This will be true if any of the values were created on lookup.
        /// </summary>
        public bool Created { get; set; }

        public string PassKey { get; set; }

        public string UserKey { get; set; }
    }
}
