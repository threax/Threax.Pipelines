using System;
using System.Diagnostics;

namespace Threax.Pipelines.Core
{
    public interface IProcessRunner
    {
        /// <summary>
        /// Run a process with output to the console.
        /// </summary>
        /// <param name="startInfo">The start info.</param>
        int RunProcessWithOutput(ProcessStartInfo startInfo);

        /// <summary>
        /// Run a process with output and return all the standard output as a string.
        /// </summary>
        /// <param name="startInfo">The start info.</param>
        /// <returns>A string with all the standard output.</returns>
        String RunProcessWithOutputGetOutput(ProcessStartInfo startInfo);
    }
}