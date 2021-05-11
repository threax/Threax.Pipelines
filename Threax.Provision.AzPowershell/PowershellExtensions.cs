using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
