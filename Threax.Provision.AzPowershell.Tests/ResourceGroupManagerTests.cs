//#define ENABLE_RESOURCE_GROUP_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Threax.Provision.AzPowershell.Tests
{
    public class ResourceGroupManagerTests
    {
        const string TestRg = "threax-prov-rg";
        const string TestLoc = "East US";

        Mockup mockup = new Mockup();

        [Fact
#if !ENABLE_RESOURCE_GROUP_TESTS
         (Skip = "Resource Group Tests Disabled")
#endif
        ]
        public async Task Create()
        {
            var manager = new ResourceGroupManager(mockup.Get<ILogger<ResourceGroupManager>>());
            await manager.Create(TestRg, TestLoc);
        }
    }
}
