using Newtonsoft.Json;
using System;
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
        /// Set this to true to always pull base images when building. Default: True.
        /// </summary>
        public bool AlwaysPull { get; set; } = true;

        private string _context;
        /// <summary>
        /// If RepoUrl is not set this will be the path to the build context. This will auto
        /// discover a .sln file in the same directory as the config file or higher. Otherwise
        /// it can be manually set. If set to a relative path it will be relative to this file.
        /// </summary>
        public string Context { get
            {
                if (this._context == null)
                {
                    var currentPath = Path.GetDirectoryName(SourceFile);
                    while (currentPath != null)
                    {
                        if (Directory.EnumerateFiles(currentPath, "*.sln", SearchOption.TopDirectoryOnly).Any())
                        {
                            this._context = currentPath;
                            break;
                        }
                        currentPath = Path.GetDirectoryName(currentPath);
                    }
                }
                return this._context;
            }
            set
            {
                this._context = value;
            }
        }

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
        /// Get the computed context value. This will resolve any relative paths, which the property does not do.
        /// </summary>
        /// <returns></returns>
        public String GetContext()
        {
            var context = this.Context;
            var basePath = Path.GetDirectoryName(SourceFile);
            return Path.Combine(basePath, context);
        }
    }
}
