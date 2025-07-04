using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using ElevatorSimulator.Core.Domain.Enums;

namespace ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;

/// <summary>
/// Handles batch trip processing - extracted from AppService
/// </summary>
public class BatchProcessor
{
    private readonly ISimulationService _simulation;
    private readonly InputHandler _inputHandler;

    public BatchProcessor(ISimulationService simulation, InputHandler inputHandler)
    {
        _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
    }

    public async Task ProcessBatchTripsAsync()
    {
        var batchRequests = new List<Request>();
        
        Console.Clear();
        Console.WriteLine("BATCH TRIP SIMULATION");
        Console.WriteLine("═══════════════════════════");
        Console.WriteLine("Add trips, then start simulation");
        Console.WriteLine();
        
        while (true)
        {
            Console.WriteLine($"Current batch: {batchRequests.Count} trip(s)");
            if (batchRequests.Any())
            {
                Console.WriteLine("Trips in batch:");
                for (int i = 0; i < batchRequests.Count; i++)
                {
                    var req = batchRequests[i];
                    var dir = req.Direction == ElevatorDirection.Up ? "UP" : "DOWN";
                    Console.WriteLine($"   {i + 1}. Floor {req.FromFloor} → {req.ToFloor} ({dir}) - {req.PassengerCount} passengers");
                }
                Console.WriteLine();
            }
            
            Console.WriteLine("Options: ENTER (add trip), 'start' (sequential), 'concurrent' (parallel), 'clear' (reset), 'back' (exit)");
            Console.Write("Choice: ");
            
            var choice = Console.ReadLine()?.Trim().ToLower();
            
            if (choice == "back") return;
            
            if (choice == "clear")
            {
                batchRequests.Clear();
                Console.WriteLine("Success: All trips cleared!");
                continue;
            }
            
            if (choice == "start")
            {
                if (!batchRequests.Any())
                {
                    Console.WriteLine("Error: No trips to simulate. Add some trips first!");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    continue;
                }
                
                await StartBatchSimulation(batchRequests, concurrent: false);
                return;
            }
            
            if (choice == "concurrent")
            {
                if (!batchRequests.Any())
                {
                    Console.WriteLine("Error: No trips to simulate. Add some trips first!");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    continue;
                }
                
                await StartBatchSimulation(batchRequests, concurrent: true);
                return;
            }
            
            // Add new trip
            try
            {
                Console.WriteLine($"\nADDING TRIP #{batchRequests.Count + 1}");
                var (fromFloor, toFloor, passengers) = _inputHandler.GetElevatorRequestInput();
                
                var newRequest = new Request(fromFloor, toFloor, passengers);
                batchRequests.Add(newRequest);
                
                Console.WriteLine($"Success: Trip #{batchRequests.Count} added!");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine("\nTRIP INPUT ERROR");
                Console.WriteLine("══════════════════");
                Console.WriteLine($"Error: {argEx.Message}");
                Console.WriteLine("\nBATCH MODE TIPS:");
                Console.WriteLine("   • Each trip needs: from floor, to floor, and passenger count");
                Console.WriteLine("   • Floors must be between 1-20 and different from each other");
                Console.WriteLine("   • Passenger count must be positive");
                Console.WriteLine("   • Take your time - you can always try again!");
                Console.WriteLine("\nPress Enter to try adding this trip again...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nUNEXPECTED ERROR ADDING TRIP");
                Console.WriteLine("═══════════════════════════════");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine("\nWhat you can do:");
                Console.WriteLine("   • Try again with simpler input");
                Console.WriteLine("   • Use 'clear' to restart your batch");
                Console.WriteLine("   • Use 'back' to return to main menu");
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }
    }

    private async Task StartBatchSimulation(List<Request> requests, bool concurrent = false)
    {
        var startTime = DateTime.Now;
        var totalPassengers = requests.Sum(r => r.PassengerCount);
        
        Console.Clear();
        var mode = concurrent ? "CONCURRENT" : "SEQUENTIAL";
        Console.WriteLine($"STARTING BATCH SIMULATION ({mode} MODE)");
        Console.WriteLine($"Total trips: {requests.Count}, Total passengers: {totalPassengers}");
        if (concurrent)
        {
            Console.WriteLine("All trips will be processed simultaneously for maximum efficiency!");
        }
        else
        {
            Console.WriteLine("Trips will be processed one by one for detailed tracking.");
        }
        Console.WriteLine();
        
        // Show trip summary
        Console.WriteLine("TRIP SUMMARY:");
        for (int i = 0; i < requests.Count; i++)
        {
            var req = requests[i];
            var dir = req.Direction == ElevatorDirection.Up ? "UP" : "DOWN";
            Console.WriteLine($"   {i + 1}. Floor {req.FromFloor} → {req.ToFloor} ({dir}) - {req.PassengerCount} passengers");
        }
        
        Console.WriteLine("\nPress ENTER to start simulation...");
        Console.ReadLine();
        
        Console.WriteLine("\nSIMULATION STARTING...");
        Console.WriteLine("=" + new string('=', 50));
        
        if (concurrent)
        {
            // Process all requests concurrently
            Console.WriteLine("\nLaunching all trips concurrently...");
            var results = await _simulation.ProcessMultipleRequestsConcurrentlyAsync(requests);
            
            // Summary of concurrent results
            var totalRemaining = results.Sum();
            if (totalRemaining > 0)
            {
                Console.WriteLine($"\nWarning: {totalRemaining} total passengers queued across all trips");
            }
            
            // Process any queued requests
            await _simulation.ProcessQueuedRequestsAsync();
        }
        else
        {
            // Process requests sequentially 
            for (int i = 0; i < requests.Count; i++)
            {
                var request = requests[i];
                Console.WriteLine($"\nProcessing Trip #{i + 1}: {request.PassengerCount} passengers from floor {request.FromFloor} to floor {request.ToFloor}");
                
                if (!_simulation.HasAvailableElevator())
                {
                    Console.WriteLine("Warning: All elevators busy! Queueing request...");
                    await _simulation.ProcessRequestAsync(request);
                }
                else
                {
                    var remainingPassengers = await _simulation.ProcessRequestAsync(request);
                    if (remainingPassengers > 0)
                    {
                        Console.WriteLine($"Warning: {remainingPassengers} passengers queued due to capacity limits");
                    }
                }
                
                await Task.Delay(1000); // Small delay between requests
            }
            
            // Process any queued requests
            await _simulation.ProcessQueuedRequestsAsync();
        }
        
        var duration = DateTime.Now - startTime;
        
        Console.WriteLine("\nBATCH SIMULATION COMPLETED!");
        Console.WriteLine("=" + new string('=', 50));
        Console.WriteLine($"Successfully processed {requests.Count} trips");
        Console.WriteLine($"Total passengers: {totalPassengers}");
        Console.WriteLine($"Total time: {duration:mm\\:ss}");
        
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
}