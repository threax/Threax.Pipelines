using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.ArmTemplates.ArmVm
{
    class ArmVm : ArmTemplate
    {
        public ArmVm(String baseName, String resourceGroup)
        {
            networkInterfaceName = $"{baseName}-ni";
            networkSecurityGroupName = $"{baseName}-nsg";
            virtualNetworkName = $"{baseName}-vnet";
            publicIpAddressName = $"{baseName}-ip";
            virtualMachineName = baseName;
            virtualMachineComputerName = baseName;
            virtualMachineRG = resourceGroup;
        }

        public String networkInterfaceName { get; set; }

        public String networkSecurityGroupName { get; set; }

        public String virtualNetworkName { get; set; }

        public String publicIpAddressName { get; set; }

        public String virtualMachineName { get; set; }

        public String virtualMachineComputerName { get; set; }

        public String virtualMachineRG { get; set; }
    }
}
