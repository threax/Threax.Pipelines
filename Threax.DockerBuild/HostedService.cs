using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Threax.K8sDeploy.Controller;

namespace Threax.K8sDeploy
{
    class HostedService : IHostedService
    {
        private IHostApplicationLifetime lifetime;
        private IController controller;

        public HostedService(IHostApplicationLifetime lifetime, IController controller)
        {
            this.lifetime = lifetime;
            this.controller = controller;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await controller.Run();

            this.lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
