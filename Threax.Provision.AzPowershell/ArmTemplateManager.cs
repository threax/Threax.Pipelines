using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class ArmTemplateManager : IArmTemplateManager
    {
        private readonly ILogger<ArmTemplateManager> logger;

        public ArmTemplateManager(ILogger<ArmTemplateManager> logger)
        {
            this.logger = logger;
        }

        public async Task ResourceGroupDeployment(String resourceGroupName, String templateFile, String templateParameterFile, Object args)
        {
            //Setup Args
            var pwshArgs = new List<KeyValuePair<String, Object>>()
                { new KeyValuePair<string, object>("ResourceGroupName", resourceGroupName) }
                .Concat(SetupArgs(ref templateFile, ref templateParameterFile, args));

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Resources");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("New-AzResourceGroupDeployment", pwshArgs);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error creating Arm Template '{templateFile}' in Resource Group '{resourceGroupName}'.");
        }

        public Task ResourceGroupDeployment(String resourceGroupName, String templateFile, Object args)
        {
            return ResourceGroupDeployment(resourceGroupName, templateFile, null, args);
        }

        public Task ResourceGroupDeployment(String resourceGroupName, String templateFile, String templateParametersFile)
        {
            return ResourceGroupDeployment(resourceGroupName, templateFile, templateParametersFile, new Object());
        }

        public Task ResourceGroupDeployment(String resourceGroupName, String templateFile)
        {
            return ResourceGroupDeployment(resourceGroupName, templateFile, null);
        }

        public Task ResourceGroupDeployment(String resourceGroupName, ArmTemplate armTemplate)
        {
            return ResourceGroupDeployment(resourceGroupName, armTemplate.GetTemplatePath(), armTemplate.GetParametersPath(), armTemplate);
        }

        public async Task SubscriptionDeployment(String location, String templateFile, String templateParameterFile, Object args)
        {
            var objectArgs = SetupArgs(ref templateFile, ref templateParameterFile, args);
            var pwshArgs = new List<KeyValuePair<String, Object>>()
                { new KeyValuePair<string, object>("Location", location) }
                .Concat(objectArgs);

            //This should work, but library load error
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Resources");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("New-AzDeployment", pwshArgs);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error creating Arm Template Deployment '{templateFile}' in Location '{location}'.");
        }

        public Task SubscriptionDeployment(String location, String templateFile, Object args)
        {
            return SubscriptionDeployment(location, templateFile, null, args);
        }

        public Task SubscriptionDeployment(String location, String templateFile, String templateParametersFile)
        {
            return SubscriptionDeployment(location, templateFile, templateParametersFile, new Object());
        }

        public Task SubscriptionDeployment(String location, String templateFile)
        {
            return SubscriptionDeployment(location, templateFile, null);
        }

        public Task SubscriptionDeployment(String resourceGroupName, ArmTemplate armTemplate)
        {
            return SubscriptionDeployment(resourceGroupName, armTemplate.GetTemplatePath(), armTemplate.GetParametersPath(), armTemplate);
        }

        private static IEnumerable<KeyValuePair<string, object>> SetupArgs(ref string templateFile, ref string templateParameterFile, Object args)
        {
            var mainArgs = new List<KeyValuePair<String, Object>>()
            {
                new KeyValuePair<string, object>("TemplateFile", templateFile = Path.GetFullPath(templateFile))
            };

            if (templateParameterFile != null)
            {
                templateParameterFile = Path.GetFullPath(templateParameterFile);
                mainArgs.Add(new KeyValuePair<string, object>("TemplateParameterFile", templateParameterFile));
            }

            var pwshArgs = mainArgs.Concat(TypeHelper.GetPropertiesAndValues(args).Where(i => i.Value != null)); //Only values that aren't null
            return pwshArgs;
        }
    }
}
