using System;
using System.Collections.Generic;
using System.Reflection;

namespace Threax.ConsoleApp
{
    public class ControllerFinder<IControllerType, CommandNotFoundType>
    {
        public Type GetControllerType(String name)
        {
            return GetControllerType(name, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// List all controller types from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>An enumerable over the matching controller types.</returns>
        public Type GetControllerType(String name, Assembly assembly)
        {
            foreach (var type in GetControllerTypes(assembly))
            {
                if (type.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return type;
                }
            }

            return typeof(CommandNotFoundType);
        }

        /// <summary>
        /// List all controller types from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>An enumerable over the matching controller types.</returns>
        private IEnumerable<Type> GetControllerTypes(Assembly assembly)
        {
            var iContollerType = typeof(IControllerType);
            foreach (var type in assembly.GetTypes())
            {
                if (iContollerType.IsAssignableFrom(type) && type != iContollerType)
                {
                    yield return type;
                }
            }
        }
    }
}
