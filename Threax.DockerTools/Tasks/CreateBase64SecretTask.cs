using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Tasks
{
    class CreateBase64SecretTask : ICreateBase64SecretTask
    {
        public CreateBase64SecretTask()
        {

        }

        public Task CreateSecret(int size, String path)
        {
            String base64;
            using (var numberGen = RandomNumberGenerator.Create()) //This is more portable than from services since that does not work on linux correctly
            {
                var bytes = new byte[size];
                numberGen.GetBytes(bytes);

                base64 = Convert.ToBase64String(bytes);
            }

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None)))
            {
                writer.Write(base64);
            }

            return Task.CompletedTask;
        }
    }
}
