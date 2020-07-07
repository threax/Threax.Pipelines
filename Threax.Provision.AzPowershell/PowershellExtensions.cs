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

namespace Threax.Provision.AzPowershell
{
    /// <summary>
    /// Extensions to make using powershell easier.
    /// Based on https://docs.microsoft.com/en-us/archive/blogs/kebab/executing-powershell-scripts-from-c
    /// Also https://github.com/PowerShell/PowerShell/tree/master/docs/host-powershell
    /// </summary>
    public static class PowershellExtensions
    {
        public static Task<PSDataCollection<PSObject>> RunAsync(this PowerShell pwsh, PSDataCollection<PSObject> output = null, int timeout = 0, int sleepInterval = 100)
        {
            var task = new TaskCompletionSource<PSDataCollection<PSObject>>();

            ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    if (output == null)
                    {
                        output = new PSDataCollection<PSObject>();
                    }
                    var result = pwsh.BeginInvoke<PSObject, PSObject>(null, output);

                    var sw = new Stopwatch();
                    sw.Start();
                    while (result.IsCompleted == false)
                    {
                        Thread.Sleep(sleepInterval);

                        if (timeout > 0 && sw.ElapsedMilliseconds > timeout)
                        {
                            throw new TimeoutException($"Timeout of {timeout}ms exceeded running powershell command.");
                        }
                    }

                    task.SetResult(output);
                }
                catch(Exception ex)
                {
                    task.SetException(ex);
                }
            });

            return task.Task;
        }

        public static PowerShell PrintInformationStream(this PowerShell pwsh, ILogger logger)
        {
            pwsh.Streams.Information.DataAdded += (sender, e) =>
            {
                var dyn = sender as PSDataCollection<InformationRecord>;
                var msg = dyn[e.Index];
                logger.LogInformation(msg.ToString());
            };
            return pwsh;
        }

        public static PowerShell PrintErrorStream(this PowerShell pwsh, ILogger logger)
        {
            pwsh.Streams.Error.DataAdded += (sender, e) =>
            {
                var dyn = sender as PSDataCollection<ErrorRecord>;
                var msg = dyn[e.Index];
                logger.LogError(msg.ToString());
            };
            return pwsh;
        }

        public static void ThrowOnErrors(this PowerShell pwsh, String message)
        {
            if (pwsh.HadErrors)
            {
                var error = new StringBuilder(message);
                error.AppendLine();
                error.AppendLine("Powershell Error:");
                error.AppendLine();
                foreach (var err in pwsh.Streams.Error)
                {
                    error.AppendLine(err.ToString());
                }

                throw new InvalidPowershellOperation(error.ToString(), pwsh.Streams.Error);
            }
        }

        public static void SetUnrestrictedExecution(this PowerShell pwsh)
        {
            pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
        }

        public static void AddParamLine(this PowerShell pwsh, object props)
        {
            pwsh.AddParamLine(TypeHelper.GetPropertiesAndValues(props));
        }

        public static void AddParamLine(this PowerShell pwsh, IEnumerable<KeyValuePair<string, object>> props)
        {
            var paramLine = new StringBuilder("param(");
            var leading = "";
            foreach (var prop in props)
            {
                paramLine.Append(leading);
                paramLine.Append("$");
                paramLine.Append(prop.Key);
                leading = ", ";
            }

            paramLine.Append(")");
            pwsh.AddScript(paramLine.ToString());

            foreach (var prop in props)
            {
                pwsh.AddParameter(prop.Key, prop.Value);
            }
        }

        public static void AddCommandWithParams(this PowerShell pwsh, String com, object props)
        {
            pwsh.AddCommandWithParams(com, TypeHelper.GetPropertiesAndValues(props));
        }

        public static void AddCommandWithParams(this PowerShell pwsh, String com, IEnumerable<KeyValuePair<string, object>> props)
        {
            var command = new StringBuilder($"{com} ");
            foreach (var arg in props)
            {
                command.Append($"-{arg.Key} ${arg.Key} ");
            }
            pwsh.AddScript(command.ToString());
        }
    }
}
