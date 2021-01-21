using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.DockerTools.Tasks;
using Threax.Pipelines.Core;
using Threax.Pipelines.Docker;

namespace Threax.DockerTools.Controller
{
    class RunController : IController
    {
        private readonly IRunTask runTask;

        public RunController(IRunTask runTask)
        {
            this.runTask = runTask;
        }

        public Task Run()
        {
            return runTask.Run();
        }
    }
}
