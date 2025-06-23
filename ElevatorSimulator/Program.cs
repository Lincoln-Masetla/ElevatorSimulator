using ElevatorSimulator.Application.Services;
using ElevatorSimulator.Presentation.Console;
using ElevatorSimulator.Domain.Interfaces;
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
        // Register services with interfaces (DIP compliance)
        services.AddSingleton<IElevatorObserverService, ElevatorObserverService>();
        services.AddSingleton<IElevatorService, ElevatorService>();
        services.AddSingleton<IDispatchService, DispatchService>();
        services.AddSingleton<ISimulationService, SimulationService>();
        
        // Register UI and application services
        services.AddSingleton<ConsoleUI>();
        services.AddSingleton<AppService>();
    }
}