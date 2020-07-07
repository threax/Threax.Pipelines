using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Threax.K8sDeploy.Controller;

namespace Threax.K8sDeploy
{
    static class ControllerFinder
    {
        public static Type GetControllerType(String name)
        {
            return GetControllerType(name, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// List all controller types from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>An enumerable over the matching controller types.</returns>
        public static Type GetControllerType(String name, Assembly assembly)
        {
            var iContollerType = typeof(IController);
            foreach (var type in GetControllerTypes(assembly))
            {
                if (type.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return type;
                }
            }

            return typeof(CommandNotFoundController);
        }

        /// <summary>
        /// List all controller types from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>An enumerable over the matching controller types.</returns>
        private static IEnumerable<Type> GetControllerTypes(Assembly assembly)
        {
            var iContollerType = typeof(IController);
            foreach(var type in assembly.GetTypes())
            {
                if (iContollerType.IsAssignableFrom(type) && type != iContollerType)
                {
                    yield return type;
                }
            }
        }
    }
}
