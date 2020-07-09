using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Resources
{
    public class SqlDatabase
    {
        public String Name { get; set; }

        public String ConnectionStringName { get; set; }

        public String OwnerConnectionStringName { get; set; }
    }
}
