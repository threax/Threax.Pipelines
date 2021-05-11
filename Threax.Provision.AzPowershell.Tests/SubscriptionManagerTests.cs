using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Threax.Provision.AzPowershell.Tests
{
    public class SubscriptionManagerTests
    {
        static readonly Guid Subscription = Guid.Empty; //Set to a real subscription guid

        Mockup mockup = new Mockup();

        [Fact
#if !ENABLE_SUBSCRIPTION_TESTS
         (Skip = "Subscription Tests Disabled")
#endif
        ]
        public async Task SetContext()
        {
            var manager = new SubscriptionManager(mockup.Get<ILogger<SubscriptionManager>>());
            await manager.SetContext(Subscription);
        }

        [Fact]
        public async Task SetContextFail()
        {
            var manager = new SubscriptionManager(mockup.Get<ILogger<SubscriptionManager>>());
            await Assert.ThrowsAnyAsync<InvalidPowershellOperation>(async () => await manager.SetContext(Guid.Empty));
        }
    }
}
