using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision
{
    public class ResourceDefinition
    {
        public List<Object> Resources { get; set; } = new List<object>();

        /// <summary>
        /// Sort any IOrderedResource instances in this object.
        /// </summary>
        public void SortResources()
        {
            this.Resources.Sort((l, r) =>
            {
                var left = l as IOrderedResource;
                var right = r as IOrderedResource;

                if (left == null && right == null)
                {
                    return 0;
                }

                if (left == null && right != null)
                {
                    return 1;
                }

                if (left != null && right == null)
                {
                    return -1;
                }

                return left.GetSortOrder() - right.GetSortOrder();
            });
        }
    }
}
