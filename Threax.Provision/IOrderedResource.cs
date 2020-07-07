using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision
{
    public interface IOrderedResource
    {
        int GetSortOrder();
    }
}
