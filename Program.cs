using Microsoft.Extensions.Hosting;
using AIPlugins.AzureFunctions.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services
            .AddScoped<IKernel>((providers) =>
            {
                // This will be called each time a new Kernel is needed

                // Get a logger instance
                ILogger<IKernel> logger = providers
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger<IKernel>();

                // Register your AI Providers...
                var appSettings = AppSettings.LoadSettings();
                IKernel kernel = new KernelBuilder()
                    .WithChatCompletionService(appSettings.Kernel)
                    .WithLogger(logger)
                    .Build();

                return kernel;
            })
            .AddScoped<IAIPluginRunner, AIPluginRunner>();
    })
    .Build();

host.Run();
