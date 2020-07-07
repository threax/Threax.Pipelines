using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MakeSecureStringAttribute : Attribute
    {
    }
}
