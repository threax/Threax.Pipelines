using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.CheapAzure.Resources
{
    public class Compute
    {
        public String Name { get; set; }

        public bool AlwaysOn { get; set; } = false;
    }
}
