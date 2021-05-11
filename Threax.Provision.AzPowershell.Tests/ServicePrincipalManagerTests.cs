//#define ENABLE_SERVICE_PRINCIPAL_MANAGER_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell.Tests
{
    public class ServicePrincipalManagerTests
    {
        private const String TestDisplayName = "threax-prov-test-user";

        Mockup mockup = new Mockup();
        Config config;

        public ServicePrincipalManagerTests(ITestOutputHelper output)
        {
            mockup.AddCommonMockups(output);
            config = mockup.Get<Config>();
        }

        [Fact
#if !ENABLE_SERVICE_PRINCIPAL_MANAGER_TESTS
         (Skip = "Service Principal Manager Tests Disabled")
#endif
        ]
        public async Task Create()
        {
            var manager = new ServicePrincipalManager(mockup.Get<IShellRunner>());
            await manager.CreateServicePrincipal(TestDisplayName, config.Subscription.ToString(), config.ResourceGroup);
        }

        [Fact
#if !ENABLE_SERVICE_PRINCIPAL_MANAGER_TESTS
         (Skip = "Service Principal Manager Tests Disabled")
#endif
        ]
        public async Task Exists()
        {
            var manager = new ServicePrincipalManager(mockup.Get<IShellRunner>());
            var result = await manager.Exists(TestDisplayName);
            Assert.True(result);
        }

        [Fact
#if !ENABLE_SERVICE_PRINCIPAL_MANAGER_TESTS
         (Skip = "Service Principal Manager Tests Disabled")
#endif
        ]
        public async Task Remove()
        {
            var manager = new ServicePrincipalManager(mockup.Get<IShellRunner>());
            await manager.Remove(TestDisplayName);
        }
    }
}
