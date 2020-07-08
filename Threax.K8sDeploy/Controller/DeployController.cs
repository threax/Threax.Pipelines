using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Threax.DockerBuildConfig;
using Threax.DeployConfig;
using Threax.Pipelines.Core;
using Threax.Pipelines.Docker;

namespace Threax.K8sDeploy.Controller
{
    class DeployController : IController
    {
        private const String Namespace = "default";

        private readonly DeploymentConfig deployConfig;
        private readonly BuildConfig buildConfig;
        private readonly ILogger logger;
        private readonly IProcessRunner processRunner;
        private readonly IKubernetes k8SClient;
        private readonly IConfigFileProvider configFileProvider;
        private readonly IOSHandler osHandler;
        private readonly IImageManager imageManager;

        public DeployController(DeploymentConfig deployConfig, BuildConfig buildConfig, ILogger<DeployController> logger, IProcessRunner processRunner, IKubernetes k8sClient, IConfigFileProvider configFileProvider, IOSHandler iOSHandler, IImageManager imageManager)
        {
            this.deployConfig = deployConfig;
            this.buildConfig = buildConfig;
            this.logger = logger;
            this.processRunner = processRunner;
            this.k8SClient = k8sClient;
            this.configFileProvider = configFileProvider;
            this.osHandler = iOSHandler;
            this.imageManager = imageManager;
        }

        public Task Run()
        {
            var nameTest = deployConfig.Name ?? throw new InvalidOperationException($"You must provide a '{nameof(DeploymentConfig.Name)}' property on your 'Deploy' property.");

            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);

            logger.LogInformation($"Redeploying '{image}' with tag '{taggedImageName}'.");

            var volumes = new List<V1Volume>();
            var volumeMounts = new List<V1VolumeMount>();

            SetupVolumes(volumes, volumeMounts);
            MountAppSettings(volumes, volumeMounts);
            SetupSecrets(volumes, volumeMounts);

            var userId = deployConfig.User != null ? long.Parse(deployConfig.User) : default(long?);
            var groupId = deployConfig.Group != null ? long.Parse(deployConfig.Group) : default(long?);

            var deployment = CreateDeployment(deployConfig.Name, taggedImageName, userId, groupId, volumes, volumeMounts);
            var service = CreateService(deployConfig.Name);
            var ingress = CreateIngress(deployConfig.Name, $"{deployConfig.Name}.{deployConfig.Domain}");

            var deployed = k8SClient.CreateOrReplaceNamespacedDeployment(deployment, Namespace);
            k8SClient.CreateOrReplaceNamespacedService(service, Namespace);
            k8SClient.CreateOrReplaceNamespacedIngress1(ingress, Namespace);

            FindPod(deployed);

            return Task.CompletedTask;
        }

        private void SetupVolumes(List<V1Volume> volumes, List<V1VolumeMount> volumeMounts)
        {
            if (deployConfig.Volumes != null)
            {
                //Ensure app data path exists (will need to handle permissions here too eventually).
                foreach (var vol in deployConfig.Volumes?.Where(i => i.Value.Type == PathType.Directory))
                {
                    var path = deployConfig.GetAppDataPath(vol.Value.Source);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    if (vol.Value.ManagePermissions)
                    {
                        osHandler.SetPermissions(path, deployConfig.User, deployConfig.Group);
                    }
                }

                volumes.AddRange(deployConfig.Volumes?.Select(i => new V1Volume()
                {
                    Name = i.Key.ToLowerInvariant(),
                    HostPath = new V1HostPathVolumeSource()
                    {
                        Path = osHandler.CreateDockerPath(deployConfig.GetAppDataPath(i.Value.Source)),
                        Type = i.Value.Type.ToString()
                    }
                }));

                volumeMounts.AddRange(deployConfig.Volumes?.Select(i => new V1VolumeMount()
                {
                    MountPath = i.Value.Destination,
                    Name = i.Key.ToLowerInvariant()
                }));
            }
        }

        private void MountAppSettings(List<V1Volume> volumes, List<V1VolumeMount> volumeMounts)
        {
            if (deployConfig.AutoMountAppSettings)
            {
                volumes.Add(new V1Volume()
                {
                    Name = "k8sconfig-appsettings-json",
                    ConfigMap = new V1ConfigMapVolumeSource()
                    {
                        Name = deployConfig.Name
                    }
                });

                volumeMounts.Add(new V1VolumeMount()
                {
                    Name = "k8sconfig-appsettings-json",
                    MountPath = deployConfig.AppSettingsMountPath,
                    SubPath = deployConfig.AppSettingsSubPath
                });

                var configMap = CreateConfigMap(deployConfig.Name, new Dictionary<string, string>() { { "appsettings.Production.json", configFileProvider.GetConfigText() } });
                k8SClient.CreateOrReplaceNamespacedConfigMap(configMap, Namespace);
            }
        }

        private void SetupSecrets(List<V1Volume> volumes, List<V1VolumeMount> volumeMounts)
        {
            if (deployConfig.Secrets != null)
            {
                //Create any secrets that have a source set
                foreach (var secret in deployConfig.Secrets.Where(i => !String.IsNullOrWhiteSpace(i.Value.Source)))
                {
                    var secretPath = deployConfig.GetConfigPath(secret.Value.Source);
                    var data = File.ReadAllText(secretPath);

                    var v1Secret = new V1Secret()
                    {
                        ApiVersion = "v1",
                        Kind = "Secret",
                        Type = "Opaque",
                        Metadata = new V1ObjectMeta()
                        {
                            Name = secret.Value.GetSecretName(deployConfig.Name, secret.Key),
                        },
                        StringData = new Dictionary<String, string>()
                        {
                            { Path.GetFileName(secretPath), data }
                        }
                    };

                    k8SClient.CreateOrReplaceNamespacedSecret(v1Secret, Namespace);
                }

                volumes.AddRange(deployConfig.Secrets.Select(i =>
                    new V1Volume()
                    {
                        Name = $"k8sconfig-secret-{i.Key.ToLowerInvariant()}",
                        Secret = new V1SecretVolumeSource()
                        {
                            SecretName = i.Value.GetSecretName(deployConfig.Name, i.Key)
                        }
                    }));

                volumeMounts.AddRange(deployConfig.Secrets.Select(i =>
                    new V1VolumeMount()
                    {
                        Name = $"k8sconfig-secret-{i.Key.ToLowerInvariant()}",
                        MountPath = i.Value.Destination,
                        SubPath = i.Value.Type == PathType.File ? Path.GetFileName(i.Value.Destination) : null
                    }));
            }
        }

        private void FindPod(V1Deployment deployed)
        {
            var key = "app";
            if (deployed.Spec.Selector.MatchLabels.TryGetValue(key, out var value))
            {
                var pods = k8SClient.ListNamespacedPod(Namespace, labelSelector: $"{key}={value}");

                var latestPod = pods.Items.Where(i => i.Metadata.DeletionTimestamp == null).FirstOrDefault(); //Find not deleted pod

                var podJsonPath = deployConfig.GetConfigPath(deployConfig.PodJsonFile);
                logger.LogInformation($"Writing pod info to '{podJsonPath}'.");
                using (var streamWriter = new StreamWriter(File.Open(podJsonPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)))
                {
                    var podJson = JsonConvert.SerializeObject(latestPod);
                    streamWriter.Write(podJson);
                    streamWriter.Close();
                }
            }
        }

        private V1ConfigMap CreateConfigMap(String name, Dictionary<String, String> data)
        {
            return new V1ConfigMap()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = name
                },
                Data = data
            };
        }

        private Networkingv1beta1Ingress CreateIngress(String name, String host)
        {
            return new Networkingv1beta1Ingress()
            {
                ApiVersion = "networking.k8s.io/v1beta1",
                Kind = "Ingress",
                Metadata = new V1ObjectMeta
                {
                    Name = name,
                },
                Spec = new Networkingv1beta1IngressSpec()
                {
                    Tls = new List<Networkingv1beta1IngressTLS>() {
                        new Networkingv1beta1IngressTLS() {
                            Hosts = new List<string>(){ host },
                            SecretName = $"{deployConfig.Domain}-tls"
                        }
                    },
                    Rules = new List<Networkingv1beta1IngressRule>() {
                        new Networkingv1beta1IngressRule() {
                            Host = host,
                            Http = new Networkingv1beta1HTTPIngressRuleValue() {
                                Paths = new List<Networkingv1beta1HTTPIngressPath>() {
                                    new Networkingv1beta1HTTPIngressPath() {
                                        Backend = new Networkingv1beta1IngressBackend() {
                                            ServiceName = name,
                                            ServicePort = 80
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private V1Service CreateService(String name)
        {
            return new V1Service()
            {
                ApiVersion = "v1",
                Kind = "Service",
                Metadata = new V1ObjectMeta()
                {
                    Name = name
                },
                Spec = new V1ServiceSpec()
                {
                    Selector = new Dictionary<String, String>() {
                        { "app", name }
                    },
                    Ports = new List<V1ServicePort>() {
                        new V1ServicePort() {
                            Port = 80,
                            TargetPort = 5000
                        }
                    }
                }
            };
        }

        private V1Deployment CreateDeployment(String name, String image, long? user, long? group, IEnumerable<V1Volume> volumes, IEnumerable<V1VolumeMount> volumeMounts)
        {
            var deployment = new V1Deployment()
            {
                ApiVersion = "apps/v1",
                Kind = "Deployment",
                Metadata = new V1ObjectMeta()
                {
                    Name = name
                },
                Spec = new V1DeploymentSpec()
                {
                    Replicas = 1,
                    Selector = new V1LabelSelector()
                    {
                        MatchLabels = new Dictionary<String, String>() {
                            { "app", name }
                        }
                    },
                    Template = new V1PodTemplateSpec()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Labels = new Dictionary<String, String>() {
                                { "app", name }
                            }
                        },
                        Spec = new V1PodSpec()
                        {
                            Volumes = volumes?.ToList(),
                            Containers = new List<V1Container>() {
                                new V1Container() {
                                    Name = "app",
                                    Image = image,
                                    Env = new List<V1EnvVar>() {
                                        new V1EnvVar() {
                                            Name = "ASPNETCORE_URLS",
                                            Value = "http://*:5000"
                                        }
                                    },
                                    ImagePullPolicy = "IfNotPresent",
                                    Ports = new List<V1ContainerPort>() {
                                        new V1ContainerPort() {
                                            ContainerPort = 5000
                                        }
                                    },
                                    VolumeMounts = volumeMounts?.ToList()
                                }
                            },
                            SecurityContext = new V1PodSecurityContext()
                            {
                                RunAsUser = user,
                                RunAsGroup = group,
                                RunAsNonRoot = true
                            }
                        }
                    },
                },
            };

            if (!String.IsNullOrEmpty(deployConfig.InitCommand))
            {
                deployment.Spec.Template.Spec.InitContainers = new List<V1Container>()
                {
                    new V1Container() {
                        Name = "app-init",
                        Command = new List<String>() { "sh", "-c", deployConfig.InitCommand },
                        Image = image,
                        Env = new List<V1EnvVar>() {
                            new V1EnvVar() {
                                Name = "ASPNETCORE_URLS",
                                Value = "http://*:5000"
                            }
                        },
                        ImagePullPolicy = "IfNotPresent",
                        Ports = new List<V1ContainerPort>() {
                            new V1ContainerPort() {
                                ContainerPort = 5000
                            }
                        },
                        VolumeMounts = volumeMounts?.ToList(),
                    }
                };
            }

            return deployment;
        }
    }
}
