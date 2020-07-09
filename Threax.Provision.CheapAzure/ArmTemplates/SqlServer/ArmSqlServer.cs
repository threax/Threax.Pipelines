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
        public ArmSqlServer(String serverName, String administratorLogin, SecureString password)
        {
            this.serverName = serverName;
            this.administratorLogin = administratorLogin;
            this.administratorLoginPassword = password;
        }

        public string serverName { get; set; }

        public string administratorLogin { get; set; }

        public SecureString administratorLoginPassword { get; set; }
    }
}
