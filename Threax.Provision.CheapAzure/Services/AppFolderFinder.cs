using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Threax.Provision.CheapAzure.Services
{
    class AppFolderFinder : IAppFolderFinder
    {
        public AppFolderFinder()
        {

        }

        public String AppUserFolder => Path.Combine(GetUserHomePath(), ".threaxprovision");

        /// <summary>
        /// Get a temporary file path in the user's ~/.threaxprovision/temp folder.
        /// </summary>
        /// <returns></returns>
        public String GetTempProvisionPath()
        {
            var tempFolder = Path.Combine(AppUserFolder, "temp");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            return Path.Combine(tempFolder, Guid.NewGuid().ToString());
        }

        private String GetUserHomePath()
        {
            //Thanks to MiffTheFox at https://stackoverflow.com/questions/1143706/getting-the-path-of-the-home-directory-in-c

            return (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }
    }
}
