using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SampleProvisioner.Services
{
    class StringGenerator : IDisposable, IStringGenerator
    {
        RandomNumberGenerator numberGen;

        public StringGenerator()
        {
            numberGen = RandomNumberGenerator.Create();
        }

        public void Dispose()
        {
            numberGen.Dispose();
        }

        public String CreateBase64String(int numBytes)
        {
            var bytes = new byte[numBytes];
            numberGen.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
