using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threax.DockerBuild
{
    interface IController
    {
        Task Run();
    }
}
