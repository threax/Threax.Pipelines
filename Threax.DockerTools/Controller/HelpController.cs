using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threax.DockerTools.Controller
{
    class HelpController : IController
    {
        private ILogger logger;

        public HelpController(ILogger<HelpController> logger)
        {
            this.logger = logger;
        }

        public Task Run()
        {
            logger.LogInformation("Help");

            return Task.CompletedTask;
        }
    }
}
