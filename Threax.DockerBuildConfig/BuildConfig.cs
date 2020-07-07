using Newtonsoft.Json;
using System;
using System.IO;

namespace Threax.DockerBuildConfig
{
    public class BuildConfig
    {
        public BuildConfig(String sourceFile)
        {
            this.SourceFile = sourceFile;
            this.ClonePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.SourceFile), "src"));
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
        /// The path to clone files to.
        /// </summary>
        [JsonIgnore]
        public String ClonePath { get; private set; }

        /// <summary>
        /// Set this to true to always pull base images when building. Default: True.
        /// </summary>
        public bool AlwaysPull { get; set; } = true;

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
    }
}
