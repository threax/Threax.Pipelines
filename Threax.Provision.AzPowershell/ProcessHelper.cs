using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    static class ProcessHelper
    {
        public static void RunProcessWithOutput(ProcessStartInfo startInfo, ILogger logger, string errorMessage, int? okExitCode = 0)
        {
            List<String> errors = null;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            using var process = Process.Start(startInfo);
            process.ErrorDataReceived += (s, e) =>
            {
                if (errors == null)
                {
                    errors = new List<string>();
                }

                if (!String.IsNullOrEmpty(e.Data))
                {
                    errors.Add(e.Data);
                    logger.LogError(e.Data);
                }
            };
            process.OutputDataReceived += (s, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    logger.LogInformation(e.Data);
                }
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();

            if (okExitCode.HasValue && process.ExitCode != okExitCode)
            {
                var sb = new StringBuilder();
                if (errorMessage != null)
                {
                    sb.AppendLine(errorMessage);
                }
                foreach (var error in errors)
                {
                    sb.AppendLine(error);
                }
                throw new InvalidOperationException(sb.ToString());
            }
        }

        public static String CreateCommandArgs(String com, IEnumerable<KeyValuePair<string, object>> props)
        {
            var command = new StringBuilder($"-c \"{com} ");
            foreach (var arg in props)
            {
                var value = arg.Value?.ToString();
                if (value != null)
                {
                    value.Replace("'", "''"); //Escape single quotes. This should be all we need for powershell. https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_quoting_rules?view=powershell-7
                    command.Append($"-{arg.Key} '{value}' ");
                }
            }
            command.Append("\"");
            return command.ToString();
        }
    }
}
