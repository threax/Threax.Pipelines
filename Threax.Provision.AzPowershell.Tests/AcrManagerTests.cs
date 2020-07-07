//#define ENABLE_ACR_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Threax.Provision.AzPowershell.Tests
{
    public class AcrManagerTests
    {
        const string TestAcr = "threaxacr";
        const string TestRg = "threax-rg";
        const string TestRegion = "East US";

        Mockup mockup = new Mockup();

        [Fact
#if !ENABLE_ACR_TESTS
         (Skip = "ACR Tests Disabled")
#endif
        ]
        public async Task CreateAcr()
        {
            var manager = new AcrManager(mockup.Get<ILogger<AcrManager>>());
            await manager.Create(TestAcr, TestRg, TestRegion, "Basic");
        }

        [Fact]
        public async Task IsNameAvailable()
        {
            var manager = new AcrManager(mockup.Get<ILogger<AcrManager>>());
            await manager.IsNameAvailable(TestAcr); //Mostly just testing that this runs without exceptions. Can't know the result ahead of time.
        }

        [Fact]
        public async Task GetAcrCredential()
        {
            var manager = new AcrManager(mockup.Get<ILogger<AcrManager>>());
            var creds = await manager.GetAcrCredential(TestAcr, TestRg); //Mostly just testing that this runs without exceptions. Can't know the result ahead of time.
        }
    }
}
