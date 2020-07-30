using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public class ServicePrincipal
    {
        /// <summary>
        /// The id to use to register the service principal in key vaults and other resources. This is only included when newly created.
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// The secret to use to connect to the service principal. This is only returned when the service principal is newly created.
        /// </summary>
        public SecureString Secret { get; set; }

        /// <summary>
        /// The application id for the service principal.
        /// </summary>
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// The display name of the service principal.
        /// </summary>
        public String DisplayName { get; set; }
    }
}
