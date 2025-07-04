using ElevatorSimulator.Presentation.Console;
using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using ElevatorSimulator.Core.Application.Common.Validators;
using ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;

namespace ElevatorSimulator.Core.Application.Features.SimulationControl.Services;

/// <summary>
/// Main application service - simplified and focused on orchestration
/// </summary>
public class AppService
{
    private readonly ISimulationService _simulation;
    private readonly ConsoleUI _ui;
    private readonly ElevatorRequestValidator _validator;
    private readonly InputHandler _inputHandler;
    private readonly BatchProcessor _batchProcessor;

    public AppService(ISimulationService simulation, ConsoleUI ui)
    {
        _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _validator = new ElevatorRequestValidator();
        _inputHandler = new InputHandler();
        _batchProcessor = new BatchProcessor(simulation, _inputHandler);
    }

    public async Task RunAsync()
    {
        ShowWelcomeScreen();

        while (true)
        {
            ShowCurrentStatus();
            ShowRequestPrompt();
            
            var input = Console.ReadLine()?.Trim();
            
            if (input?.ToLower() == "exit") break;

            if (input?.ToLower() == "batch")
            {
                await _batchProcessor.ProcessBatchTripsAsync();
                continue;
            }
            
            if (input?.ToLower() == "logs")
            {
                ShowLogs();
                continue;
            }
            
            try
            {
                List<Request> requests = ParseInput(input);
                
                // Validate all requests
                foreach (var request in requests)
                {
                    var validation = _validator.Validate(new ElevatorRequest(request.ToFloor, request.PassengerCount));
                    if (!validation.IsValid)
                    {
                        Console.WriteLine("\nREQUEST VALIDATION FAILED");
                        Console.WriteLine("═══════════════════════════════");
                        Console.WriteLine($"Trip: {request.FromFloor} -> {request.ToFloor} ({request.PassengerCount} passengers)");
                        Console.WriteLine("Issues found:");
                        foreach (var error in validation.Errors)
                        {
                            Console.WriteLine($"   • {error}");
                        }
                        Console.WriteLine("\nTIPS:");
                        Console.WriteLine("   • Floors must be between 1 and 20");
                        Console.WriteLine("   • Passenger count must be positive");
                        Console.WriteLine("   • From and to floors must be different");
                        Console.WriteLine("\nPress Enter to try again...");
                        Console.ReadLine();
                        goto continueLoop;
                    }
                }

                await ProcessRequestsAsync(requests);
                
                continueLoop:;
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine("\nINPUT ERROR");
                Console.WriteLine("═══════════════");
                Console.WriteLine($"Error: {argEx.Message}");
                Console.WriteLine("\nHELP:");
                Console.WriteLine("   • Use format: '[from] [to] [passengers]' (e.g., '1 8 5')");
                Console.WriteLine("   • Or just press ENTER for guided input");
                Console.WriteLine("   • For multiple trips: '1 8 5, 3 12 2'");
                Console.WriteLine("\nPress Enter to try again...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nUNEXPECTED ERROR");
                Console.WriteLine("═══════════════════");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine("\nWhat you can do:");
                Console.WriteLine("   • Try a simpler request first");
                Console.WriteLine("   • Use guided input (just press ENTER)");
                Console.WriteLine("   • Contact support if this persists");
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        ShowGoodbyeScreen();
    }

    private List<Request> ParseInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            // Guided input mode
            var (fromFloor, toFloor, passengers) = _inputHandler.GetElevatorRequestInput();
            return new List<Request> { new Request(fromFloor, toFloor, passengers) };
        }
        else if (input.Contains(','))
        {
            // Multi-destination format
            var requests = _inputHandler.ParseMultiDestinationInput(input);
            Console.WriteLine($"\nMulti-destination request: {requests.Count} trips");
            return requests;
        }
        else
        {
            // Single shorthand format
            var (fromFloor, toFloor, passengers) = _inputHandler.ParseShorthandInput(input);
            return new List<Request> { new Request(fromFloor, toFloor, passengers) };
        }
    }

    private async Task ProcessRequestsAsync(List<Request> requests)
    {
        foreach (var request in requests)
        {
            Console.WriteLine($"\nProcessing: {request.PassengerCount} passengers from floor {request.FromFloor} to floor {request.ToFloor}");
            Console.WriteLine("=" + new string('=', 60));
            
            if (!_simulation.HasAvailableElevator())
            {
                Console.WriteLine("All elevators busy! Queueing request...");
                await _simulation.ProcessRequestAsync(request);
                Console.WriteLine("Request queued successfully.");
            }
            else
            {
                var remainingPassengers = await _simulation.ProcessRequestAsync(request);
                
                if (remainingPassengers > 0)
                {
                    Console.WriteLine($"Warning: {remainingPassengers} passengers queued due to capacity limits");
                }
                
                // Process any queued requests
                await _simulation.ProcessQueuedRequestsAsync();
            }
            
            if (requests.Count > 1) await Task.Delay(1000);
        }
        
        Console.WriteLine("=" + new string('=', 60));
        Console.WriteLine($"Success: {requests.Count} request(s) completed!");
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }

    private void ShowCurrentStatus()
    {
        var logger = (_simulation as SimulationService)?.GetLogger();
        var requestQueue = _simulation.GetRequestQueue();
        _ui.ShowElevators(_simulation.GetElevators(), logger);
        
        if (requestQueue.HasPendingRequests)
        {
            Console.WriteLine($"\nQUEUE: {requestQueue.PendingCount} request(s) waiting");
            var next = requestQueue.PeekNext();
            if (next != null)
            {
                Console.WriteLine($"   Next: {next.PassengerCount} passengers from floor {next.FromFloor} to {next.Floor}");
            }
            Console.WriteLine();
        }
    }

    private void ShowRequestPrompt()
    {
        Console.WriteLine("ELEVATOR REQUEST SYSTEM");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("INPUT OPTIONS:");
        Console.WriteLine("   • ENTER: Step-by-step guided input (recommended for beginners)");
        Console.WriteLine("   • Quick: '[from] [to] [passengers]' → Examples: '1 8 5' or '15 3 2'");
        Console.WriteLine("   • Multi: 'trip1, trip2, trip3...' → Example: '1 8 5, 3 12 2, 7 15 1'");
        Console.WriteLine();
        Console.WriteLine("SPECIAL COMMANDS:");
        Console.WriteLine("   • 'batch': Advanced batch simulation with sequential/concurrent modes");
        Console.WriteLine("   • 'logs': View recent elevator activity and system logs");
        Console.WriteLine("   • 'exit': Quit the elevator simulation");
        Console.WriteLine();
        Console.WriteLine("TIP: Not sure? Just press ENTER for guided input!");
        Console.WriteLine();
        Console.Write("Your choice -> ");
    }

    private void ShowLogs()
    {
        var logger = (_simulation as SimulationService)?.GetLogger();
        if (logger == null)
        {
            Console.WriteLine("Error: Logger not available");
            return;
        }

        Console.Clear();
        Console.WriteLine("RECENT SYSTEM LOGS");
        Console.WriteLine("════════════════════════");
        
        var logs = logger.GetRecentLogs(20);
        if (!logs.Any())
        {
            Console.WriteLine("No recent activity.");
        }
        else
        {
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
        }
        
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }

    private void ShowWelcomeScreen()
    {
        Console.Clear();
        Console.WriteLine("=== DVT ELEVATOR SIMULATOR ===");
        Console.WriteLine("Advanced elevator control system");
        Console.WriteLine("Press Enter to start...");
        Console.ReadLine();
    }

    private void ShowGoodbyeScreen()
    {
        Console.Clear();
        Console.WriteLine("=== THANK YOU ===");
        Console.WriteLine("Thanks for using DVT Elevator Simulator!");
    }
}