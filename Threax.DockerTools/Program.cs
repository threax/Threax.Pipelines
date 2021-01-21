using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DeployConfig;
using Threax.DockerBuildConfig;
using Threax.DockerTools.Controller;
using Threax.DockerTools.Tasks;
using Threax.Extensions.Configuration.SchemaBinder;
using Threax.Pipelines.Core;

namespace Threax.DockerTools
{
    class Program
    {
        public static Task<int> Main(string[] args)
        {
            var jsonConfigPath = args.Length > 1 ? args[1] : "unknown.json";

            string command = null;
            if (args.Length > 0)
            {
                command = args[0];
            }

            return AppHost
            .Setup<IController, HelpController>(command, services =>
            {
                services.AddSingleton<IArgsProvider>(s => new ArgsProvider(args));

                services.AddScoped<SchemaConfigurationBinder>(s =>
                {
                    var configBuilder = new ConfigurationBuilder();
                    configBuilder.AddJsonFile(jsonConfigPath);
                    return new SchemaConfigurationBinder(configBuilder.Build());
                });

                services.AddThreaxPipelines(o =>
                {
                    o.SetupConfigFileProvider = s => new ConfigFileProvider(jsonConfigPath);
                });
                services.AddThreaxPipelinesDocker();

                services.AddScoped<BuildConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var appConfig = new BuildConfig(jsonConfigPath);
                    config.Bind("Build", appConfig);
                    appConfig.Validate();
                    return appConfig;
                });

                services.AddScoped<DeploymentConfig>(s =>
                {
                    var config = s.GetRequiredService<SchemaConfigurationBinder>();
                    var deployConfig = new DeploymentConfig(jsonConfigPath);
                    config.Bind("Deploy", deployConfig);
                    deployConfig.Validate();
                    return deployConfig;
                });

                services.AddLogging(o =>
                {
                    o.AddConsole();
                });

                services.AddScoped<ICreateBase64SecretTask, CreateBase64SecretTask>();
                services.AddScoped<ICreateCertificateTask, CreateCertificateTask>();
                services.AddScoped<ILoadTask, LoadTask>();
                services.AddScoped<IRunTask, RunTask>();
                services.AddScoped<IStopContainerTask, StopContainerTask>();
            })
            .Run(c => c.Run());
        }
    }
}
