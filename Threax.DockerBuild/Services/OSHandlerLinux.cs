using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.K8sDeploy.Services
{
    public class OSHandlerLinux : IOSHandler
    {
        private readonly IProcessRunner processRunner;

        public OSHandlerLinux(IProcessRunner processRunner)
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
            this.processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("chown", $"-R {user}:{group} {path}"));
            this.processRunner.RunProcessWithOutput(new System.Diagnostics.ProcessStartInfo("chmod", $"700 {path}"));
        }
    }
}
