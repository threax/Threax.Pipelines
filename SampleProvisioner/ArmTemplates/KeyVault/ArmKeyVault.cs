using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Threax.Provision.AzPowershell;

namespace SampleProvisioner.ArmTemplates.KeyVault
{
    class ArmKeyVault : ArmTemplate
    {
        public ArmKeyVault(String nameFromTemplate, String location, String tenant)
        {
            this.nameFromTemplate = nameFromTemplate;
            this.location = location;
            this.tenant = tenant;
        }

        public String nameFromTemplate { get; set; }

        public string location { get; }
        
        public string tenant { get; }
    }
}
