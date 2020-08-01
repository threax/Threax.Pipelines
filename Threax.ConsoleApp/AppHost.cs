using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Threax.ConsoleApp
{
    public abstract class AppHost
    {
        private readonly Action<ServiceCollection> setup;

        protected AppHost(Action<ServiceCollection> setup)
        {
            this.setup = setup;
        }

        public static NoControllerAppHost Setup(Action<ServiceCollection> setup)
        {
            return new NoControllerAppHost(setup);
        }

        public static AppHost<IControllerType> Setup<IControllerType, CommandNotFoundType>(String command, Action<ServiceCollection> setup)
        {
            return new AppHost<IControllerType>(s =>
            {
                setup(s);

                var controllerFinder = new ControllerFinder<IControllerType, CommandNotFoundType>();
                var controllerType = typeof(CommandNotFoundType);
                //Determine which controller to use.
                if (command != null)
                {
                    controllerType = controllerFinder.GetControllerType(command);
                }
                s.TryAddScoped(typeof(IControllerType), controllerType);
            });
        }

        protected async Task<int> DoRun(Func<IServiceScope, Task<int>> run)
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
                Console.Write("Exception");
                Console.ForegroundColor = current;
                Console.Write(": ");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                var inner = ex.InnerException;
                while (inner != null)
                {
                    current = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("InnerException");
                    Console.ForegroundColor = current;
                    Console.Write(": ");
                    Console.WriteLine(inner.Message);
                    Console.WriteLine(inner.StackTrace);
                    inner = inner.InnerException;
                }
                result = -1;
            }

            sw.Stop();
            Console.WriteLine($"Tasks took {sw.Elapsed}");

            return result;
        }
    }

    public class AppHost<IControllerType> : AppHost
    {
        public AppHost(Action<ServiceCollection> setup) : base(setup)
        {
        }

        public Task<int> Run(Func<IControllerType, IServiceScope, Task> run)
        {
            return DoRun(async scope =>
            {
                var controller = scope.ServiceProvider.GetRequiredService<IControllerType>();
                await run(controller, scope);
                return 0;
            });
        }

        public Task<int> Run(Func<IControllerType, IServiceScope, Task<int>> run)
        {
            return DoRun(scope =>
            {
                var controller = scope.ServiceProvider.GetRequiredService<IControllerType>();
                return run(controller, scope);
            });
        }

        public Task<int> Run(Func<IControllerType, Task> run)
        {
            return DoRun(async scope =>
            {
                var controller = scope.ServiceProvider.GetRequiredService<IControllerType>();
                await run(controller);
                return 0;
            });
        }

        public Task<int> Run(Func<IControllerType, Task<int>> run)
        {
            return DoRun(scope =>
            {
                var controller = scope.ServiceProvider.GetRequiredService<IControllerType>();
                return run(controller);
            });
        }
    }

    public class NoControllerAppHost : AppHost
    {
        public NoControllerAppHost(Action<ServiceCollection> setup) : base(setup)
        {
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
    }
}
