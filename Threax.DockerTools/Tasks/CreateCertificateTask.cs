using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Threax.DockerTools.Tasks
{
    class CreateCertificateTask : ICreateCertificateTask
    {
        public Task Execute(String commonName, int expirationMonths, String path)
        {
            using (var rsa = RSA.Create()) // generate asymmetric key pair
            {
                var request = new CertificateRequest($"cn={commonName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                //Thanks to Muscicapa Striata for these settings at
                //https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));
                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                //Create the cert
                byte[] certBytes;
                using (var cert = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-2)), new DateTimeOffset(DateTime.UtcNow.AddMonths(expirationMonths))))
                {
                    certBytes = cert.Export(X509ContentType.Pfx);
                }
                using (var stream = File.Open(path, FileMode.Create))
                {
                    stream.Write(certBytes);
                }
            }

            return Task.CompletedTask;
        }
    }
}
