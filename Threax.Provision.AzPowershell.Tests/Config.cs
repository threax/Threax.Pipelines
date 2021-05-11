using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.AzPowershell.Tests
{
    class Config
    {
        public Guid Subscription { get; set; }

        public String ResourceGroup { get; set; } = "threax-prov-rg";

        public Guid UserGuid { get; set; }
    }
}
