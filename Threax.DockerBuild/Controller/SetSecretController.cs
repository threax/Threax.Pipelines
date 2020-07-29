using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.DeployConfig;

namespace Threax.DockerBuild.Controller
{
    class SetSecretController : IController
    {
        private readonly DeploymentConfig deploymentConfig;

        public SetSecretController(DeploymentConfig deploymentConfig)
        {
            this.deploymentConfig = deploymentConfig;
        }

        public Task Run()
        {
            

            return Task.CompletedTask;
        }
    }
}
