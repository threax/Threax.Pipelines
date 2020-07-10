﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IWebAppManager
    {
        Task<WebAppInfo> CreateWebAppIdentity(string Name, string ResourceGroupName);
        Task<WebAppInfo> GetWebApp(string Name, string ResourceGroupName);
        Task SetHostnames(string Name, string ResourceGroupName, IEnumerable<string> HostNames);
    }
}