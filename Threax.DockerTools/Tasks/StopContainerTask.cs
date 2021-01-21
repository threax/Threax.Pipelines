using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Threax.Pipelines.Core;

namespace Threax.DockerTools.Tasks
{
    class StopContainerTask : IStopContainerTask
    {
        private readonly IProcessRunner processRunner;

        public StopContainerTask(
            IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        public void StopContainer(String name)
        {
            //It is ok if this fails, probably means it wasn't running
            processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"rm {name} --force"));
        }
    }
}
