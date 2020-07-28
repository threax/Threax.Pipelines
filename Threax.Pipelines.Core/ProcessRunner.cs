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

                process.WaitForExit();

                return process.ExitCode;
            }
        }

        public String RunProcessWithOutputGetOutput(ProcessStartInfo startInfo)
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
            }

            return output.ToString();
        }
    }
}
