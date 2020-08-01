using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.ArmTemplates.ArmVm
{
    class ArmVm : ArmTemplate
    {
        public ArmVm(String baseName, String resourceGroup, String adminUsername, String adminPublicKey)
        {
            this.networkInterfaceName = $"{baseName}-ni";
            this.networkSecurityGroupName = $"{baseName}-nsg";
            this.virtualNetworkName = $"{baseName}-vnet";
            this.publicIpAddressName = $"{baseName}-ip";
            this.virtualMachineName = baseName;
            this.virtualMachineComputerName = baseName;
            this.virtualMachineRG = resourceGroup;
            this.adminUsername = adminUsername;
            this.adminPublicKey = adminPublicKey;
        }

        public String networkInterfaceName { get; set; }

        public String networkSecurityGroupName { get; set; }

        public String virtualNetworkName { get; set; }

        public String publicIpAddressName { get; set; }

        public String virtualMachineName { get; set; }

        public String virtualMachineComputerName { get; set; }

        public String virtualMachineRG { get; set; }

        public String adminUsername { get; set; }

        public String adminPublicKey { get; set; }
    }
}
