using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threax.Provision.Processing
{
    public interface IProvisioner
    {
        Task ProcessResources(ResourceDefinition resourceDefinition, IServiceScope scope);
    }
}