using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class ArmTemplateManager : IArmTemplateManager
    {
        private readonly IShellRunner shellRunner;

        public ArmTemplateManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public Task ResourceGroupDeployment(String resourceGroupName, String templateFile, String templateParameterFile, Object args)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Resources");
            var commands = new FormattableString[] { $"New-AzResourceGroupDeployment -Name {Guid.NewGuid()} -ResourceGroupName {resourceGroupName}" }.Concat(SetupArgs(ref templateFile, ref templateParameterFile, args));
            pwsh.AddResultCommand(commands);

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error creating Arm Template '{templateFile}' in Resource Group '{resourceGroupName}'.");
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

        public Task SubscriptionDeployment(String location, String templateFile, String templateParameterFile, Object args)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Resources");
            var commands = new FormattableString[] { $"New-AzDeployment -Name {Guid.NewGuid()} -Location {location}" }.Concat(SetupArgs(ref templateFile, ref templateParameterFile, args));
            pwsh.AddResultCommand(commands);

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error creating Arm Template Deployment '{templateFile}' in Location '{location}'.");
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

        private static IEnumerable<FormattableString> SetupArgs(ref string templateFile, ref string templateParameterFile, Object args)
        {
            templateFile = Path.GetFullPath(templateFile);

            var mainArgs = new List<FormattableString>()
            {
                $" -TemplateFile {templateFile}"
            };

            if (templateParameterFile != null)
            {
                templateParameterFile = Path.GetFullPath(templateParameterFile);
                mainArgs.Add($" -TemplateParameterFile {templateParameterFile}");
            }

            foreach (var prop in TypeHelper.GetPropertiesAndValues(args).Where(i => i.Value != null))
            {
                mainArgs.Add($" -{new RawProcessString(prop.Key)} {prop.Value}");
            }

            return mainArgs;
        }
    }
}
