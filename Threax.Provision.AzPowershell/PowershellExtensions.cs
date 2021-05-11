using System;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public static class PowershellExtensions
    {
        public static void SetUnrestrictedExecution(this IShellCommandBuilder pwsh)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                //Only do this on Windows
                //pwsh.AddCommand($"Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
            }
        }
    }
}
