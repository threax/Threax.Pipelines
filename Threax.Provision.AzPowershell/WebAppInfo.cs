using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public class WebAppInfo
    {
        /// <summary>
        /// The object id of the identity for this app if one exists. Otherwise null.
        /// </summary>
        public Guid? IdentityObjectId { get; set; }
    }
}
