using ElevatorSimulator.Core.Application.Features.SimulationControl.Services;
using ElevatorSimulator.Core.Application.Features.ElevatorManagement.Services;
using ElevatorSimulator.Core.Application.Features.ElevatorManagement.Repositories;
using ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;
using ElevatorSimulator.Presentation.Console;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ElevatorSimulator;

/// <summary>
/// Entry point implementing dependency injection and clean architecture
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Create host with dependency injection
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            // Run the application
            var app = host.Services.GetRequiredService<AppService>();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Configure dependency injection following SOLID principles
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register domain services
        services.AddSingleton<IElevatorRepository, ElevatorRepository>();
        services.AddSingleton<IQueueService, QueueService>();
        
        // Register application services with interfaces (DIP compliance)
        services.AddSingleton<IElevatorObserverService, ElevatorObserverService>();
        services.AddSingleton<IElevatorService, ElevatorService>();
        services.AddSingleton<IDispatchService, DispatchService>();
        services.AddSingleton<ISimulationService, SimulationService>();
        
        // Register UI and application services
        services.AddSingleton<ConsoleUI>();
        services.AddSingleton<AppService>();
    }
}