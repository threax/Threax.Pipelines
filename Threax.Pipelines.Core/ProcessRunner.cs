using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Threax.Pipelines.Core
{
    public class ProcessRunner : IProcessRunner
    {
        public int RunProcessWithOutput(ProcessStartInfo startInfo)
        {
            return RunProcessWithOutput(startInfo, null);
        }

        public int RunProcessWithOutput(ProcessStartInfo startInfo, Action<Process> processCreated)
        {
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            using (var process = Process.Start(startInfo))
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data);
                    }
                };
                process.OutputDataReceived += (s, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                processCreated?.Invoke(process);

                process.WaitForExit();

                return process.ExitCode;
            }
        }

        public String RunProcessWithOutputGetOutput(ProcessStartInfo startInfo)
        {
            return RunProcessWithOutputGetOutput(startInfo, out _);
        }

        public String RunProcessWithOutputGetOutput(ProcessStartInfo startInfo, out int exitCode)
        {
            StringBuilder output = new StringBuilder();
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            using (var process = Process.Start(startInfo))
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data);
                    }
                };
                process.OutputDataReceived += (s, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        Console.WriteLine(e.Data);
                    }
                };
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            return output.ToString();
        }
    }
}
