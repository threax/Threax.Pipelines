using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public class VaultCertificate
    {
        public String KeyId { get; set; }

        public String SecretId { get; set; }

        public String Thumbprint { get; set; }
    }
}
