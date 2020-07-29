using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Pipelines.Core
{
    public class OSHandlerUnix : IOSHandler
    {
        private readonly IProcessRunner processRunner;

        public OSHandlerUnix(IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        public string CreateDockerPath(string path)
        {
            return path;
        }

        public void SetPermissions(string path, string user, string group)
        {
            //sudo chown -R 19999:19999 /data/app/id
            //sudo chmod 700 /data/app/id

            //Dunno if this will work
            int exitCode;
            exitCode = this.processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("chown", $"-R {user}:{group} {path}"));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the chown.");
            }
            exitCode = this.processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("chmod", $"-R 700 {path}"));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the chmod.");
            }
        }
    }
}
