using System;
using System.Collections.Generic;

namespace Threax.Provision.Processing
{
    public interface IResourceProcessorResolver
    {
        void AddResourceProcessors(IEnumerable<Type> processors);
        bool TryGetProcessorType(Type resourceType, out Type processorType);
    }
}