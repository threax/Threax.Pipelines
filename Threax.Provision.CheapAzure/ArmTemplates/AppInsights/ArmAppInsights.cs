using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.ArmTemplates.AppInsights
{
    class ArmAppInsights : ArmTemplate
    {
        public ArmAppInsights(String name, String regionId)
        {
            this.nameFromTemplate = name;
            this.regionId = regionId;
        }

        public String nameFromTemplate { get; set; }

        public String regionId { get; set; }

        public String type { get; set; } = "web";
    }
}
