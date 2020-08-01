using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;

namespace Threax.DockerTools.Controller
{
    class CommandNotFoundController : IController
    {
        private ILogger logger;
        private readonly IArgsProvider argsProvider;

        public CommandNotFoundController(ILogger<CommandNotFoundController> logger, IArgsProvider argsProvider)
        {
            this.logger = logger;
            this.argsProvider = argsProvider;
        }

        public async Task Run()
        {
            logger.LogInformation($"Command not found {argsProvider.Args[0]}.");
        }
    }
}
