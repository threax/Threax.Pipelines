using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.ArmTemplates.SqlServer
{
    class ArmSqlServer : ArmTemplate
    {
        public ArmSqlServer(String serverName, String administratorLogin, String password, String vnetName, String vnetSubnetName)
        {
            this.serverName = serverName;
            this.administratorLogin = administratorLogin;
            this.administratorLoginPassword = password;
            this.vnetSubnetName = vnetSubnetName;
            this.vnetName = vnetName;
        }

        public string serverName { get; set; }

        public string administratorLogin { get; set; }

        public String administratorLoginPassword { get; set; }

        public string vnetSubnetName { get; }
        
        public String vnetRuleName { get; set; } = "VnetAccess";

        public String vnetName { get; set; }
    }
}
