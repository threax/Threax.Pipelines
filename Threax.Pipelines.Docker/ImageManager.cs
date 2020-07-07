using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Threax.Pipelines.Core;

namespace Threax.Pipelines.Docker
{
    public class ImageManager : IImageManager
    {
        private readonly IProcessRunner processRunner;

        public ImageManager(IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        public string FindLatestImage(string image, string baseTag, string currentTag)
        {
            //Get the tags from docker
            var args = $"inspect --format=\"{{{{json .RepoTags}}}}\" {image}:{currentTag}";
            var startInfo = new ProcessStartInfo("docker", args);
            var json = processRunner.RunProcessWithOutputGetOutput(startInfo);
            var tags = JsonConvert.DeserializeObject<List<String>>(json);

            //Remove any tags that weren't set by this software
            tags.Remove($"{image}:{currentTag}");
            var tagFilter = $"{image}:{baseTag}";
            tags = tags.Where(i => i.StartsWith(tagFilter)).ToList();
            tags.Sort(); //Docker seems to store these in order, but sort them by their names, the tags are date based and the latest will always be last

            var latestDateTag = tags.LastOrDefault();

            if (latestDateTag == null)
            {
                throw new InvalidOperationException($"Cannot find a tag in the format '{tagFilter}' on image '{image}'.");
            }

            return latestDateTag;
        }
    }
}
