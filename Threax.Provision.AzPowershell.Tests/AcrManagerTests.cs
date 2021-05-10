//#define ENABLE_ACR_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Threax.ProcessHelper;
using Xunit.Abstractions;

namespace Threax.Provision.AzPowershell.Tests
{
    public class AcrManagerTests
    {
        const string TestAcr = "threaxacr";
        const string TestRg = "threax-rg";
        const string TestRegion = "East US";

        Mockup mockup = new Mockup();

        public AcrManagerTests(ITestOutputHelper output)
        {
            mockup.AddCommonMockups(output);
        }

        [Fact
#if !ENABLE_ACR_TESTS
         (Skip = "ACR Tests Disabled")
#endif
        ]
        public async Task CreateAcr()
        {
            var manager = new AcrManager(mockup.Get<IShellRunner>());
            await manager.Create(TestAcr, TestRg, TestRegion, "Basic");
        }

        [Fact]
        public async Task IsNameAvailable()
        {
            var manager = new AcrManager(mockup.Get<IShellRunner>());
            await manager.IsNameAvailable(TestAcr); //Mostly just testing that this runs without exceptions. Can't know the result ahead of time.
        }

        [Fact]
        public async Task GetAcrCredential()
        {
            var manager = new AcrManager(mockup.Get<IShellRunner>());
            var creds = await manager.GetAcrCredential(TestAcr, TestRg); //Mostly just testing that this runs without exceptions. Can't know the result ahead of time.
        }
    }
}
