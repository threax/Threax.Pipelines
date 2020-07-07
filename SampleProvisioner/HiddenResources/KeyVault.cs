using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision;

namespace SampleProvisioner.HiddenResources
{
    public class KeyVault : IOrderedResource
    {
        public int GetSortOrder() => 1;
    }
}
