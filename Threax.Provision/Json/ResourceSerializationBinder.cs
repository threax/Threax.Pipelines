using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision
{
    public class ResourceSerializationBinder : ISerializationBinder
    {
        private Dictionary<String, Type> knownTypes = new Dictionary<string, Type>();
        private Dictionary<Type, String> knownTypesReverse = new Dictionary<Type, String>();

        public void RegisterType<T>(String name)
        {
            RegisterType(name, typeof(T));
        }

        public void RegisterType(String name, Type type)
        {
            knownTypes[name] = type;
            knownTypesReverse[type] = name;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = "";
            if (!knownTypesReverse.TryGetValue(serializedType, out typeName))
            {
                throw new InvalidOperationException($"Type '{serializedType.GetType().FullName}' cannot be found in the type map. This is potentially unsafe and the current operation was aborted.");
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            Type result;
            if (!knownTypes.TryGetValue(typeName, out result))
            {
                throw new InvalidOperationException($"Type '{typeName}' cannot be found in the type map. This is potentially unsafe and the current operation was aborted.");
            }

            return result;
        }
    }
}
