using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using Threax.DockerBuildConfig;
using Threax.Extensions.Configuration.SchemaBinder;
using Threax.K8sDeploy.Controller;
using Threax.K8sDeployConfig;

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

                    services.AddThreaxPipelines();

                    services.AddScoped<IKubernetes>(s =>
                    {
                        var config = KubernetesClientConfiguration.BuildDefaultConfig();
                        IKubernetes client = new Kubernetes(config);
                        return client;
                    });

                    services.AddScoped<DeploymentConfig>(s =>
                    {
                        var config = s.GetRequiredService<SchemaConfigurationBinder>();
                        var appConfig = new DeploymentConfig(jsonConfigPath);
                        config.Bind("Deploy", appConfig);
                        appConfig.Validate();
                        return appConfig;
                    });

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
                    if(args.Length > 0)
                    {
                        var command = args[0];
                        controllerType = ControllerFinder.GetControllerType(command);
                    }
                    services.AddScoped(typeof(IController), controllerType);
                });
    }
}
