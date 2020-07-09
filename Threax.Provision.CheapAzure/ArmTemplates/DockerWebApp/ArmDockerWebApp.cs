using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Text;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.ArmTemplates.DockerWebApp
{
    class ArmDockerWebApp : ArmTemplate
    {
        public String subscriptionId { get; set; }

        public String nameFromTemplate { get; set; }

        public String location { get; set; }

        public String hostingPlanName { get; set; }

        public String serverFarmResourceGroup { get; set; }

        public bool alwaysOn { get; set; }

        public String linuxFxVersion { get; set; }

        public String dockerRegistryUrl { get; set; }

        public String dockerRegistryUsername { get; set; }

        public SecureString dockerRegistryPassword { get; set; }

        public String loadCertificates { get; set; }

        //public String dockerRegistryStartupCommand { get; set; }
    }
}
