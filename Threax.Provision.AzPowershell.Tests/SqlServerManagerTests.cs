//#define ENABLE_SQL_SERVER_TESTS

using System;
using Xunit;
using Threax.Provision.AzPowershell;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Threax.Provision.AzPowershell.Tests
{
    public class SqlServerManagerTests : IDisposable
    {
        const string TestSqlServer = "threax-prov";
        const string TestRg = "threax-prov-rg";
        const string TestRegion = "East US";
        const string TestFirewallRule = "test-rule";
        const string Ip = "192.168.1.1";

        Mockup mockup = new Mockup();

        public void Dispose()
        {
            mockup.Dispose();
        }

        [Fact
#if !ENABLE_SQL_SERVER_TESTS
         (Skip = "Sql Server Tests Disabled")
#endif
        ]
        public async Task CreateSqlServer()
        {
            using var numberGen = RandomNumberGenerator.Create();

            var bytes = new byte[64];
            numberGen.GetBytes(bytes);

            var secretString = Convert.ToBase64String(bytes);

            var user = secretString.Substring(0, secretString.Length / 2);
            var pass = secretString.Substring(secretString.Length / 2) + "!2Ab"; //Last bit ensures complexity

            var manager = new SqlServerManager(mockup.Get<ILogger<SqlServerManager>>());
            await manager.Create(TestSqlServer, TestRg, TestRegion, user, pass);
        }

        [Fact
#if !ENABLE_SQL_SERVER_TESTS
         (Skip = "Sql Server Tests Disabled")
#endif
        ]
        public async Task SetServerFirewallRule()
        {
            var manager = new SqlServerManager(mockup.Get<ILogger<SqlServerManager>>());
            await manager.SetFirewallRule(TestFirewallRule, TestSqlServer, TestRg, Ip, Ip);
        }

        [Fact
#if !ENABLE_SQL_SERVER_TESTS
         (Skip = "Sql Server Tests Disabled")
#endif
        ]
        public async Task RemoveServerFirewallRule()
        {
            var manager = new SqlServerManager(mockup.Get<ILogger<SqlServerManager>>());
            await manager.RemoveFirewallRule(TestFirewallRule, TestSqlServer, TestRg);
        }

        [Fact
#if !ENABLE_SQL_SERVER_TESTS
         (Skip = "Sql Server Tests Disabled")
#endif
        ]
        public async Task FirewallRuleManagerTests()
        {
            using var firewallRuleManager = new SqlServerFirewallRuleManager(new SqlServerManager(mockup.Get<ILogger<SqlServerManager>>()));

            await firewallRuleManager.Unlock(TestSqlServer, TestRg, Ip, Ip);
            await firewallRuleManager.Unlock(TestSqlServer, TestRg, Ip, Ip);
        }
    }
}
