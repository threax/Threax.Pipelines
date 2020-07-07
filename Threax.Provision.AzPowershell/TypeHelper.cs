using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    class TypeHelper
    {
        public static IEnumerable<KeyValuePair<String, Object>> GetPropertiesAndValues(Object instance)
        {
            foreach (var prop in instance.GetType().GetTypeInfo().DeclaredProperties.Where(i => i.CanRead))
            {
                yield return KeyValuePair.Create(prop.Name, prop.GetGetMethod().Invoke(instance, new object[0]));
            }
        }
    }
}
