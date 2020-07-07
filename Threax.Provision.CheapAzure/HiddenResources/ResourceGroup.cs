using System;
using System.Collections.Generic;
using System.Text;
using Threax.Provision;

namespace Threax.Provision.CheapAzure.HiddenResources
{
    public class ResourceGroup : IOrderedResource
    {
        public ResourceGroup(String name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public int GetSortOrder() => 0;
    }
}
