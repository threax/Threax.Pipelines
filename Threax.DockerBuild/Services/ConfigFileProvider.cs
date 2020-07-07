using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Threax.DockerBuildConfig;

namespace Threax.K8sDeploy.Services
{
    class ConfigFileProvider : IConfigFileProvider
    {
        private readonly BuildConfig appConfig;

        public ConfigFileProvider(BuildConfig appConfig)
        {
            this.appConfig = appConfig;
        }

        public String GetConfigText()
        {
            var path = appConfig.SourceFile;

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
