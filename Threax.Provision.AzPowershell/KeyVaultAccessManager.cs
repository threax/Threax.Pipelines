using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class KeyVaultAccessManager : IDisposable, IKeyVaultAccessManager
    {
        private readonly IKeyVaultManager keyVaultManager;
        private List<KeyVaultRuleInfo> createdRules = new List<KeyVaultRuleInfo>();

        public KeyVaultAccessManager(IKeyVaultManager keyVaultManager)
        {
            this.keyVaultManager = keyVaultManager;
        }

        public async Task Unlock(String keyVaultName, Guid userId)
        {
            //If this user is already unlocked, do nothing
            if (createdRules.Any(i => i.KeyVaultName == keyVaultName && i.UserId == userId))
            {
                return;
            }

            await keyVaultManager.UnlockSecrets(keyVaultName, userId);
            this.createdRules.Add(new KeyVaultRuleInfo()
            {
                KeyVaultName = keyVaultName,
                UserId = userId
            });
        }

        public void Dispose()
        {
            var ruleTasks = createdRules.Select(i => Task.Run(() => this.keyVaultManager.LockSecrets(i.KeyVaultName, i.UserId))).ToList();
            foreach (var task in ruleTasks)
            {
                task.GetAwaiter().GetResult();
            }
        }

        class KeyVaultRuleInfo
        {
            public string KeyVaultName { get; set; }
            public Guid UserId { get; set; }
        }
    }
}
