using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Threax.ConsoleApp
{
    public class AppHost
    {
        private readonly Action<ServiceCollection> setup;

        protected AppHost(Action<ServiceCollection> setup)
        {
            this.setup = setup;
        }

        public static AppHost Setup(Action<ServiceCollection> setup)
        {
            return new AppHost(setup);
        }

        public Task<int> Run(Func<IServiceScope, Task> run)
        {
            return DoRun(async scope =>
            {
                await run(scope);
                return 0;
            });
        }

        public Task<int> Run(Func<IServiceScope, Task<int>> run)
        {
            return DoRun(run);
        }

        private async Task<int> DoRun(Func<IServiceScope, Task<int>> run)
        {
            int result;
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                var services = new ServiceCollection();

                setup(services);

                using (var serviceProvider = services.BuildServiceProvider())
                using (var scope = serviceProvider.CreateScope())
                {
                    result = await run(scope);
                }
            }
            catch (Exception ex)
            {
                var current = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ForegroundColor = current;
                Console.Write(": ");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                result = -1;
            }

            sw.Stop();
            Console.WriteLine($"Tasks took {sw.Elapsed}");

            return result;
        }
    }
}
