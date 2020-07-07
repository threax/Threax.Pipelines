using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Threax.Provision.Hidden;

namespace Threax.Provision.Processing
{
    public class ResourceProcessorResolver : IResourceProcessorResolver
    {
        private Dictionary<Type, Type> resourceProcessor = new Dictionary<Type, Type>();

        public void AddResourceProcessors(IEnumerable<Type> processors)
        {
            foreach (var processorType in processors)
            {
                if (processorType == typeof(IResourceProcessor))
                {
                    throw new InvalidOperationException($"Cannot use {nameof(IResourceProcessor)} directly.");
                }
                if (!typeof(IResourceProcessor).IsAssignableFrom(processorType))
                {
                    throw new InvalidOperationException($"Resource type must Implement IResourceProcessor<T>.");
                }
                var face = processorType.GetInterfaces().Where(i => typeof(IResourceProcessor).IsAssignableFrom(i)).FirstOrDefault();
                if (face == null || !face.IsGenericType)
                {
                    throw new InvalidOperationException($"Resource type must Implement IResourceProcessor<T>.");
                }

                var resourceType = face.GetGenericArguments()[0];

                if (resourceProcessor.ContainsKey(resourceType))
                {
                    throw new InvalidOperationException($"The resource type {resourceType.FullName} was found more than once in the processor set. It should only be defined once. Seen types are '{processorType.FullName}' and '{resourceProcessor[resourceType].FullName}'");
                }

                resourceProcessor.Add(resourceType, processorType);
            }
        }

        public bool TryGetProcessorType(Type resourceType, out Type processorType)
        {
            return resourceProcessor.TryGetValue(resourceType, out processorType);
        }
    }
}
