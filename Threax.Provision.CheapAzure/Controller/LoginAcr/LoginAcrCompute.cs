using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Threax.Pipelines.Core;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.Resources;

namespace Threax.Provision.CheapAzure.Controller.LoginAcr
{
    class LoginAcrCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly IAcrManager acrManager;
        private readonly IProcessRunner processRunner;
        private readonly ILogger<LoginAcrCompute> logger;

        public LoginAcrCompute(Config config, IAcrManager acrManager, IProcessRunner processRunner, ILogger<LoginAcrCompute> logger)
        {
            this.config = config;
            this.acrManager = acrManager;
            this.processRunner = processRunner;
            this.logger = logger;
        }

        public async Task Execute(Compute resource)
        {
            logger.LogInformation($"Logging into ACR '{config.AcrName}'.");

            var acrCreds = await acrManager.GetAcrCredential(config.AcrName, config.ResourceGroup);

            var passwordPath = Path.GetTempFileName();

            try
            {
                using (var passwordStream = new StreamWriter(File.Open(passwordPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)))
                {
                    passwordStream.Write(acrCreds.Password);
                }

                var startInfo = new ProcessStartInfo("pwsh", $"-c \"cat {passwordPath} | docker login {config.AcrName}.azurecr.io --username {acrCreds.Username} --password-stdin\"");
                processRunner.RunProcessWithOutput(startInfo);
            }
            finally
            {
                //Erase the file
                if (File.Exists(passwordPath))
                {
                    File.Delete(passwordPath);
                }
            }
        }
    }
}
