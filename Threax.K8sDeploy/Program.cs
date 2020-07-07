using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using Threax.DockerBuildConfig;
using Threax.Extensions.Configuration.SchemaBinder;
using Threax.K8sDeploy.Controller;
using Threax.K8sDeployConfig;
using Threax.Pipelines.Core;

namespace Threax.K8sDeploy
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var jsonConfigPath = args[1];

                    services.AddHostedService<HostedService>();

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

                    services.AddScoped<IKubernetes>(s =>
                    {
                        var config = KubernetesClientConfiguration.BuildDefaultConfig();
                        IKubernetes client = new Kubernetes(config);
                        return client;
                    });

                    services.AddScoped<DeploymentConfig>(s =>
                    {
                        var config = s.GetRequiredService<SchemaConfigurationBinder>();
                        var deployConfig = new DeploymentConfig(jsonConfigPath);
                        config.Bind("Deploy", deployConfig);
                        deployConfig.Validate();
                        return deployConfig;
                    });

                    services.AddScoped<BuildConfig>(s =>
                    {
                        var config = s.GetRequiredService<SchemaConfigurationBinder>();
                        var buildConfig = new BuildConfig(jsonConfigPath);
                        config.Bind("Build", buildConfig);
                        buildConfig.Validate();
                        return buildConfig;
                    });

                    var controllerType = typeof(HelpController);
                    //Determine which controller to use.
                    if(args.Length > 0)
                    {
                        var command = args[0];
                        controllerType = ControllerFinder.GetControllerType(command);
                    }
                    services.AddScoped(typeof(IController), controllerType);
                });
    }
}
