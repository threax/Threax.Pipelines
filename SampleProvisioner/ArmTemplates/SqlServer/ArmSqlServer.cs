using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Threax.Provision.AzPowershell;

namespace SampleProvisioner.ArmTemplates.SqlServer
{
    class ArmSqlServer : ArmTemplate
    {
        public ArmSqlServer(String serverName, String administratorLogin, String password)
        {
            this.serverName = serverName;
            this.administratorLogin = administratorLogin;
            this.administratorLoginPassword = new SecureString();
            foreach(var c in password)
            {
                this.administratorLoginPassword.AppendChar(c);
            }
            this.administratorLoginPassword.MakeReadOnly();
        }

        public string serverName { get; set; }

        public string administratorLogin { get; set; }

        public SecureString administratorLoginPassword { get; set; }
    }
}
