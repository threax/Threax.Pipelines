using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.Hidden
{
    public interface IResourceProcessor
    {

    }

}

namespace Threax.Provision
{

    public interface IResourceProcessor<T> : Hidden.IResourceProcessor
    {
        Task Execute(T resource);
    }
}
