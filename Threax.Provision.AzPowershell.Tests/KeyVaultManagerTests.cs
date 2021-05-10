#define ENABLE_KEY_VAULT_TESTS

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
    public class KeyVaultManagerTests
    {
        const string TestVault = "threax-prov-kv";
        const string TestRg = "threax-prov-rg";
        const string TestRegion = "East US";
        static readonly Guid TestGuid = Guid.Parse("e172b235-4b92-4a6f-96e9-c7097bb973db"); //Set this to a real guid.
        const string TestKey = "TestSecret";
        const string TestValue = "A Test Value";

        Mockup mockup = new Mockup();

        public KeyVaultManagerTests(ITestOutputHelper output)
        {
            mockup.AddCommonMockups(output);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task UnlockSecrets()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            await manager.UnlockSecrets(TestVault, TestGuid);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task LockSecrets()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            await manager.LockSecrets(TestVault, TestGuid);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task SetSecret()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            await manager.SetSecret(TestVault, TestKey, TestValue);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task GetSecret()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            var result = (await manager.GetSecret(TestVault, TestKey));
            Assert.Equal(result, TestValue);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task GetSecretMissing()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            var result = await manager.GetSecret(TestVault, "NotFound");
            Assert.Null(result);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task KeyVaultAccessManagerTests()
        {
            using var keyVaultAccess = new KeyVaultAccessManager(new KeyVaultManager(mockup.Get<IShellRunner>()));

            await keyVaultAccess.Unlock(TestVault, TestGuid);
            await keyVaultAccess.Unlock(TestVault, TestGuid);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task Exists()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            var result = await manager.Exists(TestVault);
            Assert.True(result);
        }

        [Fact
#if !ENABLE_KEY_VAULT_TESTS
         (Skip = "Key Vault Tests Disabled")
#endif
        ]
        public async Task ExistsNot()
        {
            var manager = new KeyVaultManager(mockup.Get<IShellRunner>());
            var result = await manager.Exists(TestVault + "doesnotexist");
            Assert.False(result);
        }
    }
}
