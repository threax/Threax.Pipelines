#define ENABLE_WEBAPP_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Threax.Provision.AzPowershell.Tests
{
    public class WebAppManagerTests
    {
        const String WebAppName = "threax-id";
        const String ResourceGroupName = "threax-rg";

        Mockup mockup = new Mockup();

        [Fact
#if !ENABLE_WEBAPP_TESTS
         (Skip = "Subscription Tests Disabled")
#endif
        ]
        public async Task GetWebApp()
        {
            var manager = new WebAppManager(mockup.Get<ILogger<WebAppManager>>());
            var appInfo = await manager.GetWebApp(WebAppName, ResourceGroupName);
        }

        [Fact
#if !ENABLE_WEBAPP_TESTS
         (Skip = "Subscription Tests Disabled")
#endif
        ]
        public async Task CreateWebAppIdentity()
        {
            var manager = new WebAppManager(mockup.Get<ILogger<WebAppManager>>());
            var appInfo = await manager.CreateWebAppIdentity(WebAppName, ResourceGroupName);
        }
    }
}
