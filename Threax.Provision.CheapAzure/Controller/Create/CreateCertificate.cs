using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.Resources;
using Threax.Provision.CheapAzure.Services;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateCertificate : IResourceProcessor<Certificate>
    {
        private readonly IStringGenerator stringGenerator;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly Config config;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;

        public CreateCertificate(IStringGenerator stringGenerator, IKeyVaultManager keyVaultManager, Config config, IKeyVaultAccessManager keyVaultAccessManager)
        {
            this.stringGenerator = stringGenerator;
            this.keyVaultManager = keyVaultManager;
            this.config = config;
            this.keyVaultAccessManager = keyVaultAccessManager;
        }

        public async Task Execute(Certificate resource)
        {
            if (String.IsNullOrEmpty(resource.Name))
            {
                throw new InvalidOperationException($"You must provide a value for '{nameof(Certificate.Name)}' in your '{nameof(Certificate)}' types.");
            }

            if (String.IsNullOrEmpty(resource.CN))
            {
                throw new InvalidOperationException($"You must provide a value for '{nameof(Certificate.CN)}' in your '{nameof(Certificate)}' types.");
            }

            await keyVaultAccessManager.Unlock(config.KeyVaultName, config.UserId);

            var existingCert = await keyVaultManager.GetCertificate(config.KeyVaultName, resource.Name);
            if (existingCert == null)
            {
                using (var rsa = RSA.Create()) // generate asymmetric key pair
                {
                    var request = new CertificateRequest($"cn={resource.CN}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    //Thanks to Muscicapa Striata for these settings at
                    //https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin
                    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));
                    request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                    //Create the cert
                    var password = stringGenerator.CreateBase64String(32);
                    byte[] certBytes;
                    using (var cert = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-2)), new DateTimeOffset(DateTime.UtcNow.AddMonths(resource.ExpirationMonths))))
                    {
                        certBytes = cert.Export(X509ContentType.Pfx, password);
                    }
                    await keyVaultManager.ImportCertificate(config.KeyVaultName, resource.Name, certBytes, password.ToSecureString());
                }
            }
        }
    }
}
