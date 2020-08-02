using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.Services;

namespace Threax.Provision.CheapAzure
{
    public class Config
    {
        /// <summary>
        /// The name of the resource group to put everything in.
        /// </summary>
        public String ResourceGroup { get; set; }

        /// <summary>
        /// The azure location to use.
        /// </summary>
        public String Location { get; set; }

        /// <summary>
        /// The current TenantId
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The current user id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The name of the sql server to create.
        /// </summary>
        public String SqlServerName { get; set; }

        /// <summary>
        /// The name of the sql database to create. This setup shares 1 db for all apps to save money.
        /// </summary>
        public String SqlDbName { get; set; }

        /// <summary>
        /// The name of the key vault to create for general infrastructure that doesn't belong to an app.
        /// </summary>
        public String InfraKeyVaultName { get; set; }

        /// <summary>
        /// The public ip of the current machine to temporarly create a sql server firewall rule for.
        /// Use GetMachineIp to get the real value of this property, since it can be looked up.
        /// </summary>
        public string MachineIp { get; set; }

        public async Task<String> GetMachineIp(IMachineIpManager machineIpManager)
        {
            if(this.MachineIp == null)
            {
                this.MachineIp = await machineIpManager.GetExternalIp();
            }

            return this.MachineIp;
        }

        /// <summary>
        /// The name of the acr to create.
        /// </summary>
        public String AcrName { get; set; }

        /// <summary>
        /// The name of the vm to provision.
        /// </summary>
        public String VmName { get; set; }

        /// <summary>
        /// The name of the app service plan to create.
        /// </summary>
        public String AppServicePlanName { get; set; }

        /// <summary>
        /// The name of the shared app insights.
        /// </summary>
        public String AppInsightsName { get; set; }

        /// <summary>
        /// The current subscription id.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The base name of the sa secret in the key vault. Default: "sqlsrv-sa"
        /// </summary>
        public string SqlSaBaseKey { get; set; } = "sqlsrv-sa";

        /// <summary>
        /// The base name of the secret in the infra kv for the vm admin. Default: 'vm-admin'
        /// </summary>
        public string VmAdminBaseKey { get; set; } = "vm-admin";

        /// <summary>
        /// The name of the key in the infra key vault that holds the known hosts key. Default: 'known-hosts'
        /// </summary>
        public string SshKnownHostKey { get; set; } = "known-hosts";

        /// <summary>
        /// The guid of the Azure Devops user to set permissions for.
        /// </summary>
        public Guid? AzDoUser { get; set; }

        /// <summary>
        /// Set this to true to allow the current user to unlock key vaults. If this is false no changes 
        /// will be made and the current user must have permissions set from somewhere else. Default: true
        /// </summary>
        public bool UnlockCurrentUserInKeyVaults { get; set; } = true;

        /// <summary>
        /// The name of the public ip for the vm.
        /// </summary>
        public String PublicIpName { get; set; }

        /// <summary>
        /// If the vm ip address is known you can set it here for a small speed improvement since it will avoid lookups.
        /// Otherwise the ip is looked up from PublicIpName. This has no effect when creating the ip.
        /// </summary>
        public String VmIpAddress { get; set; }

        /// <summary>
        /// The name of the Network Security Group.
        /// </summary>
        public String NsgName { get; set; }
    }
}
