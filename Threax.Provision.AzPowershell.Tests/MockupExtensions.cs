using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Threax.AspNetCore.Tests;
using Threax.ProcessHelper;
using Xunit.Abstractions;

namespace Threax.Provision.AzPowershell.Tests
{
    static class MockupExtensions
    {
        public static Mockup AddCommonMockups(this Mockup mockup, ITestOutputHelper output)
        {
            mockup.MockServiceCollection.AddThreaxPwshShellRunner(o =>
            {
                o.IncludeLogOutput = false;
                o.DecorateProcessRunner = r => new SpyProcessRunner(r)
                {
                    Events = new ProcessEvents()
                    {
                        ErrorDataReceived = (o, e) => { if (e.Data != null) output.WriteLine(e.Data); },
                        OutputDataReceived = (o, e) => { if (e.Data != null) output.WriteLine(e.Data); },
                    }
                };
            });

            return mockup;
        }
    }
}
