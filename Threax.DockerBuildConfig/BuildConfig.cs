using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Threax.DockerBuildConfig
{
    public class BuildConfig
    {
        public BuildConfig(String sourceFile)
        {
            this.SourceFile = Path.GetFullPath(sourceFile);
            this.ClonePath = Path.Combine(Path.GetDirectoryName(sourceFile), "src");
        }

        /// <summary>
        /// The name of the created docker image.
        /// </summary>
        public String ImageName { get; set; }

        /// <summary>
        /// The url of the repository with the app's code.
        /// </summary>
        public String RepoUrl { get; set; }

        /// <summary>
        /// The path to the dockerfile. This must be specified to use Build.
        /// </summary>
        public String Dockerfile { get; set; }

        /// <summary>
        /// The branch of the repo to use. Default: master.
        /// </summary>
        public String Branch { get; set; } = "master";

        /// <summary>
        /// The base tag of the app. This is used when looking up image builds.
        /// </summary>
        public String BaseTag { get; set; } = "threaxpipe";

        /// <summary>
        /// The path to the source file of this app config.
        /// </summary>
        [JsonIgnore]
        public String SourceFile { get; private set; }

        /// <summary>
        /// The path to clone files to. This will be used as the context if RepoUrl is set.
        /// </summary>
        [JsonIgnore]
        public String ClonePath { get; private set; }

        /// <summary>
        /// An array of images to pull when building. These will be pulled with separate docker pull commands.
        /// </summary>
        public List<String> PullImages { get; set; }

        /// <summary>
        /// Set this to true to always pull base images when building. This will add 
        /// --pull to build commands. Default: False.
        /// </summary>
        public bool PullAllImages { get; set; } = false;

        /// <summary>
        /// If RepoUrl is not set this will be the path to the build context. This can be
        /// set to a relative path from this config file.
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// A map of arguments to pass as --build-arg params when building.
        /// </summary>
        public Dictionary<String, String> Args { get; set; }

        public void Validate()
        {
            if (Branch == null)
            {
                throw new InvalidOperationException($"{nameof(Branch)} cannot be null. Please provide a value.");
            }

            if (Dockerfile == null)
            {
                throw new InvalidOperationException($"{nameof(Dockerfile)} cannot be null. Please provide a value.");
            }

            if (BaseTag == null)
            {
                throw new InvalidOperationException($"{nameof(BaseTag)} cannot be null. Please provide a value.");
            }
        }

        public String GetBuildTag()
        {
            return $"{BaseTag}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        }

        public String GetCurrentTag()
        {
            return $"{BaseTag}-current";
        }

        /// <summary>
        /// Get the computed context value. This will resolve any relative paths and handle the repo url, which the property does not do.
        /// </summary>
        /// <returns></returns>
        public String GetContext()
        {
            String basePath;
            if (RepoUrl == null)
            {
                basePath = Path.GetDirectoryName(SourceFile);
            }
            else
            {
                basePath = ClonePath;
            }

            var context = this.Context;
            if (context != null)
            {
                return Path.Combine(basePath, context);
            }
            return basePath;
        }
    }
}
