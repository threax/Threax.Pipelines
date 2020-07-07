using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Threax.Provision.AzPowershell;

namespace SampleProvisioner.ArmTemplates.ResourceGroup
{
    class ArmResourceGroup : ArmTemplate
    {
        public ArmResourceGroup(String rgName)
        {
            this.rgName = rgName;
        }

        public string rgName { get; }
    }
}
