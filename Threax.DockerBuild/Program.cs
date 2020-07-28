using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DockerBuildConfig;
using Threax.Extensions.Configuration.SchemaBinder;
using Threax.K8sDeploy.Controller;

namespace Threax.K8sDeploy
{
    class Program
    {
        public static Task<int> Main(string[] args)
        {
            var jsonConfigPath = args.Length > 1 ? args[1] : "unknown.json";

            return AppHost
            .Setup(services =>
            {
                services.AddScoped<SchemaConfigurationBinder>(s =>
                {
                    var configBuilder = new ConfigurationBuilder();
                    configBuilder.AddJsonFile(jsonConfigPath);
                    return new SchemaConfigurationBinder(configBuilder.Build());
                });

                services.AddThreaxPipelines();
                services.AddThreaxPipelinesDocker();

                services.AddScoped<BuildConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var appConfig = new BuildConfig(jsonConfigPath);
                    config.Bind("Build", appConfig);
                    appConfig.Validate();
                    return appConfig;
                });

                var controllerType = typeof(HelpController);
                //Determine which controller to use.
                if (args.Length > 0)
                {
                    var command = args[0];
                    controllerType = ControllerFinder.GetControllerType(command);
                }
                services.AddScoped(typeof(IController), controllerType);

                services.AddLogging(o =>
                {
                    o.AddConsole();
                });
            })
            .Run(async scope =>
            {
                var controller = scope.ServiceProvider.GetRequiredService<IController>();
                await controller.Run();
            });
        }
    }
}
