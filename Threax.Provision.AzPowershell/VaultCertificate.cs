using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public class VaultCertificate
    {
        public VaultCertificate(string keyId, string secretId, string thumbprint)
        {
            KeyId = keyId;
            SecretId = secretId;
            Thumbprint = thumbprint;
        }

        public String KeyId { get; set; }

        public String SecretId { get; set; }

        public String Thumbprint { get; set; }
    }
}
