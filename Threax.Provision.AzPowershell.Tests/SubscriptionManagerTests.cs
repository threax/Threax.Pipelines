//#define ENABLE_SUBSCRIPTION_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Threax.ProcessHelper;
using Xunit.Abstractions;

namespace Threax.Provision.AzPowershell.Tests
{
    public class SubscriptionManagerTests
    {
        Mockup mockup = new Mockup();
        Config config;

        public SubscriptionManagerTests(ITestOutputHelper output)
        {
            mockup.AddCommonMockups(output);
            config = mockup.Get<Config>();
        }

        [Fact
#if !ENABLE_SUBSCRIPTION_TESTS
         (Skip = "Subscription Tests Disabled")
#endif
        ]
        public async Task SetContext()
        {
            var manager = new SubscriptionManager(mockup.Get<IShellRunner>());
            await manager.SetContext(config.Subscription);
        }

        [Fact]
        public async Task SetContextFail()
        {
            var manager = new SubscriptionManager(mockup.Get<IShellRunner>());
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await manager.SetContext(Guid.Empty));
        }
    }
}
