using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class DummyKeyVaultAccessManager : IKeyVaultAccessManager
    {
        public DummyKeyVaultAccessManager()
        {
            
        }

        public Task Unlock(String keyVaultName, Guid userId)
        {
            return Task.CompletedTask;
        }
    }
}
