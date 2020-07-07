//#define ENABLE_ARM_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Threax.Provision.AzPowershell.Tests
{
    public class ArmTemplateManagerTests
    {
        const string TestRg = "threax-prov-rg";
        const string TestOverrideRg = "threax-prov-override-rg";
        const string TestLoc = "East US";

        Mockup mockup = new Mockup();

        [Fact
#if !ENABLE_ARM_TESTS
         (Skip = "Arm Tests Disabled")
#endif
        ]
        public async Task DeployRg()
        {
            var manager = new ArmTemplateManager(mockup.Get<ILogger<ArmTemplateManager>>());
            await manager.SubscriptionDeployment(TestLoc, "ResourceGroupTemplate/template.json", "ResourceGroupTemplate/parameters.json");
        }

        [Fact
#if !ENABLE_ARM_TESTS
         (Skip = "Arm Tests Disabled")
#endif
        ]
        public async Task DeployRgOverride()
        {
            var manager = new ArmTemplateManager(mockup.Get<ILogger<ArmTemplateManager>>());
            await manager.SubscriptionDeployment(TestLoc, "ResourceGroupTemplate/template.json", "ResourceGroupTemplate/parameters.json", new { rgName = TestOverrideRg });
            }

        [Fact
#if !ENABLE_ARM_TESTS
         (Skip = "Arm Tests Disabled")
#endif
        ]
        public async Task DeployKeyVault()
        {
            var manager = new ArmTemplateManager(mockup.Get<ILogger<ArmTemplateManager>>());
            await manager.ResourceGroupDeployment(TestRg, "KeyVaultTemplate/template.json", "KeyVaultTemplate/parameters.json");
        }

        [Fact
#if !ENABLE_ARM_TESTS
         (Skip = "Arm Tests Disabled")
#endif
        ]
        public async Task DeployKeyVaultOverride()
        {
            //Note that this test also shows how to use nameFromTemplate to pass any parameters that are name in the arm template.
            var manager = new ArmTemplateManager(mockup.Get<ILogger<ArmTemplateManager>>());
            await manager.ResourceGroupDeployment(TestOverrideRg, "KeyVaultTemplate/template.json", "KeyVaultTemplate/parameters.json", new { nameFromTemplate = "threax-prov-override-kv" } /*In the template this is just name. Have to use nameFromTemplate to pass that value. This does not seem to be documented anywhere.*/ );
        }
    }
}
