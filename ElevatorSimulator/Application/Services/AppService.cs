using ElevatorSimulator.Application.Validators;
using ElevatorSimulator.Presentation.Console;
using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Interfaces;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Application.Observers;

namespace ElevatorSimulator.Application.Services;

/// <summary>
/// Application service orchestrating the elevator system following clean architecture
/// </summary>
public class AppService
{
    private readonly ISimulationService _simulation;
    private readonly ConsoleUI _ui;
    private readonly ElevatorRequestValidator _validator;

    public AppService(ISimulationService simulation, ConsoleUI ui)
    {
        _simulation = simulation;
        _ui = ui;
        _validator = new ElevatorRequestValidator();
    }

    public async Task RunAsync()
    {
        ShowWelcomeScreen();

        while (true)
        {
            // Show current elevator status with observer logs and queue info
            var logger = (_simulation as SimulationService)?.GetLogger();
            var requestQueue = _simulation.GetRequestQueue();
            _ui.ShowElevators(_simulation.GetElevators(), logger);
            
            // Show queue status if there are pending requests
            if (requestQueue.HasPendingRequests)
            {
                System.Console.WriteLine($"\n⏳ QUEUE STATUS: {requestQueue.PendingCount} request(s) waiting for available elevator");
                var nextRequest = requestQueue.PeekNext();
                if (nextRequest != null)
                {
                    System.Console.WriteLine($"   📍 Next in queue: {nextRequest.PassengerCount} passenger(s) from floor {nextRequest.FromFloor} to floor {nextRequest.Floor}");
                }
                System.Console.WriteLine();
            }

            ShowRequestPrompt();
            var input = System.Console.ReadLine()?.Trim();
            
            if (input?.ToLower() == "exit") break;

            if (input?.ToLower() == "multi")
            {
                await ProcessMultiFloorScenarioAsync();
                continue;
            }
            
            if (input?.ToLower() == "batch")
            {
                await ProcessBatchTripsAsync();
                continue;
            }
            
            if (input?.ToLower() == "logs")
            {
                ShowComprehensiveLogs();
                continue;
            }
            
            // Handle guided input, shorthand format, and multi-destination format
            try
            {
                List<Request> requests;
                
                if (string.IsNullOrEmpty(input))
                {
                    // Guided input mode
                    var (fromFloor, toFloor, passengers) = GetElevatorRequestInput();
                    requests = new List<Request> { new Request(fromFloor, toFloor, passengers) };
                }
                else if (input.Contains(','))
                {
                    // Multi-destination format: "3 8 12, 7 8 12"
                    requests = ParseMultiDestinationInput(input);
                    System.Console.WriteLine($"\nMulti-destination request detected: {requests.Count} trips");
                    foreach (var req in requests)
                    {
                        System.Console.WriteLine($"  Trip: {req.PassengerCount} passengers from floor {req.FromFloor} to floor {req.ToFloor}");
                    }
                }
                else
                {
                    // Single shorthand format
                    var (fromFloor, toFloor, passengers) = ParseShorthandInput(input);
                    requests = new List<Request> { new Request(fromFloor, toFloor, passengers) };
                }

                // Validate all requests
                foreach (var request in requests)
                {
                    var validation = _validator.Validate(new ElevatorRequest(request.ToFloor, request.PassengerCount));
                    if (!validation.IsValid)
                    {
                        System.Console.WriteLine($"ERROR: {string.Join(", ", validation.Errors)}");
                        System.Console.WriteLine("Press Enter to continue...");
                        System.Console.ReadLine();
                        goto continueLoop;
                    }
                }

                // Process all requests
                var totalProcessed = 0;
                foreach (var request in requests)
                {
                    totalProcessed++;
                    var requestType = requests.Count > 1 ? "Multi" : "Single";
                    
                    if (!_simulation.HasAvailableElevator())
                    {
                        System.Console.WriteLine($"\nAll elevators are busy! Queueing request: {request.PassengerCount} passenger(s) from floor {request.FromFloor} to floor {request.ToFloor}");
                        System.Console.WriteLine("=" + new string('=', 80));
                        
                        await _simulation.ProcessRequestAsync(request); // This will queue the request
                        
                        var queue = _simulation.GetRequestQueue();
                        System.Console.WriteLine($"Request added to queue. Position: #{queue.PendingCount}");
                        System.Console.WriteLine("Your request will be processed when an elevator becomes available.");
                    }
                    else
                    {
                        if (requests.Count > 1)
                        {
                            System.Console.WriteLine($"\nProcessing trip {totalProcessed}/{requests.Count}: {request.PassengerCount} passenger(s) from floor {request.FromFloor} to floor {request.ToFloor}...");
                        }
                        else
                        {
                            System.Console.WriteLine($"\nProcessing elevator request: {request.PassengerCount} passenger(s) from floor {request.FromFloor} to floor {request.ToFloor}...");
                        }
                        System.Console.WriteLine("=" + new string('=', 80));
                        
                        // Log request start
                        var systemLogger = (_simulation as SimulationService)?.GetLogger();
                        systemLogger?.LogRequestStart(request, requestType);
                        
                        // Process request through dispatch service (which handles actual elevator movement)
                        var remainingPassengers = await _simulation.ProcessRequestAsync(request);
                        
                        // Log any capacity overflow
                        if (remainingPassengers > 0)
                        {
                            systemLogger?.LogCapacityOverflow(request.PassengerCount, request.PassengerCount - remainingPassengers, remainingPassengers);
                        }
                        
                        // Check for queued requests after completion
                        await ProcessAnyQueuedRequestsAsync();
                    }
                    
                    // Small delay between multiple requests
                    if (requests.Count > 1 && totalProcessed < requests.Count)
                    {
                        await Task.Delay(1000);
                    }
                }
                
                System.Console.WriteLine("=" + new string('=', 80));
                var completionMessage = requests.Count > 1 
                    ? $"All {requests.Count} elevator requests completed!"
                    : "Elevator request completed!";
                System.Console.WriteLine(completionMessage);
                
                // Show detailed simulation summary
                ShowSimulationSummary(requests);
                
                System.Console.WriteLine("\nPress Enter to continue...");
                System.Console.ReadLine();
                
                continueLoop:;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error processing request: {ex.Message}");
                System.Console.WriteLine("Press Enter to continue...");
                System.Console.ReadLine();
            }
        }

        ShowGoodbyeScreen();
    }

    private void ShowRequestPrompt()
    {
        System.Console.WriteLine("ELEVATOR REQUEST SYSTEM");
        System.Console.WriteLine("─────────────────────────");
        System.Console.WriteLine("Instructions:");
        System.Console.WriteLine("   • Interactive Mode: Press ENTER for guided input with hints");
        System.Console.WriteLine("   • Quick Mode: Type '[from] [to] [passengers]' (e.g., '1 8 5' or '8 3')");
        System.Console.WriteLine("   • Multi-Destination: Type '3 8 12, 7 8 12' for multiple trips");
        System.Console.WriteLine("   • Batch Mode: Type 'batch' to add multiple trips then simulate all");
        System.Console.WriteLine("   • Smart queueing: Busy elevators will queue requests intelligently");
        System.Console.WriteLine("   • Type 'multi' for 5-person scenario demo");
        System.Console.WriteLine("   • Type 'logs' to view comprehensive system logs");
        System.Console.WriteLine("   • Type 'exit' to quit");
        System.Console.WriteLine();
        System.Console.WriteLine("Building Info: 4 Elevators, 20 floors, 48 total capacity");
        System.Console.WriteLine("Quick examples: '1 15 8', '3 8 12', '1 20 25', '3 8 12, 7 8 12'");
        System.Console.WriteLine();
        System.Console.Write("Command (ENTER/numbers/batch/multi/logs/exit): ");
    }

    private void ShowWelcomeScreen()
    {
        System.Console.Clear();
        System.Console.WriteLine("=== DVT ELEVATOR SIMULATOR ===");
        System.Console.WriteLine("Simple elevator control system");
        System.Console.WriteLine("Press Enter to start...");
        System.Console.ReadLine();
    }

    private async Task ProcessMultiFloorScenarioAsync()
    {
        System.Console.WriteLine("\n🎭 MULTI-FLOOR SCENARIO DEMO");
        System.Console.WriteLine("=== 5 People Taking Same Elevator ===");
        System.Console.WriteLine("📋 Scenario: 5 people from different floors going to different destinations");
        System.Console.WriteLine("Press Enter to start demo...");
        System.Console.ReadLine();
        
        var requests = new List<Request>
        {
            new(8, 1, 3),   // 1 person from floor 3 to floor 8
            new(12, 1, 5),  // 1 person from floor 5 to floor 12
            new(6, 1, 7),   // 1 person from floor 7 to floor 6
            new(15, 1, 9),  // 1 person from floor 9 to floor 15
            new(4, 1, 11)   // 1 person from floor 11 to floor 4
        };
        
        await ProcessMultipleRequestsAsync(requests);
        
        System.Console.WriteLine("\n🎉 Multi-floor scenario completed!");
        System.Console.WriteLine("Press Enter to continue...");
        System.Console.ReadLine();
    }
    
    private async Task ProcessMultipleRequestsAsync(List<Request> requests)
    {
        var elevators = _simulation.GetElevators();
        var bestElevator = elevators.FirstOrDefault(e => e.State == Domain.Enums.ElevatorState.Idle);
        
        if (bestElevator == null)
        {
            System.Console.WriteLine("❌ No available elevator found!");
            return;
        }
        
        System.Console.WriteLine($"🎯 Dispatching Elevator {bestElevator.Id} for multi-floor journey");
        await Task.Delay(500);
        
        // Sort requests by pickup floor for efficiency
        var sortedRequests = requests.OrderBy(r => r.FromFloor).ToList();
        var currentFloor = bestElevator.CurrentFloor;
        var totalPassengers = 0;
        
        // Pick up passengers from each floor
        foreach (var request in sortedRequests)
        {
            // Move to pickup floor
            if (currentFloor != request.FromFloor)
            {
                System.Console.WriteLine($"🚀 Moving to floor {request.FromFloor} to pick up passenger...");
                await SimulateMovementAsync(bestElevator.Id, currentFloor, request.FromFloor);
                currentFloor = request.FromFloor;
            }
            
            // Pick up passenger
            System.Console.WriteLine($"🚪 Floor {request.FromFloor}: Doors opening for pickup...");
            await Task.Delay(1000);
            
            totalPassengers++;
            System.Console.WriteLine($"👤 Passenger {totalPassengers} boarding (going to floor {request.Floor})... ({totalPassengers}/{bestElevator.MaxCapacity})");
            await Task.Delay(800);
            
            System.Console.WriteLine($"🚪 Doors closing...");
            await Task.Delay(1000);
        }
        
        System.Console.WriteLine($"📊 All passengers aboard! Total: {totalPassengers} passengers");
        System.Console.WriteLine("🎯 Now delivering passengers to their destinations...");
        await Task.Delay(1000);
        
        // Drop off passengers at their destinations (sorted by floor for efficiency)
        var dropOffs = sortedRequests.OrderBy(r => r.Floor).ToList();
        var remainingPassengers = totalPassengers;
        
        foreach (var request in dropOffs)
        {
            // Move to destination floor
            if (currentFloor != request.Floor)
            {
                System.Console.WriteLine($"🚀 Moving to floor {request.Floor} for drop-off...");
                await SimulateMovementAsync(bestElevator.Id, currentFloor, request.Floor);
                currentFloor = request.Floor;
            }
            
            // Drop off passenger
            System.Console.WriteLine($"🚪 Floor {request.Floor}: Doors opening for exit...");
            await Task.Delay(1000);
            
            remainingPassengers--;
            System.Console.WriteLine($"👤 Passenger exiting at floor {request.Floor}... ({remainingPassengers} remaining)");
            await Task.Delay(600);
            
            System.Console.WriteLine($"🚪 Doors closing...");
            await Task.Delay(1000);
        }
        
        System.Console.WriteLine($"✅ Multi-floor journey completed! Elevator {bestElevator.Id} served {totalPassengers} passengers efficiently");
    }
    
    private async Task SimulateMovementAsync(int elevatorId, int fromFloor, int toFloor)
    {
        var direction = toFloor > fromFloor ? "UP" : "DOWN";
        System.Console.WriteLine($"🚀 Elevator {elevatorId} moving {direction} from floor {fromFloor} to floor {toFloor}");
        
        var currentFloor = fromFloor;
        while (currentFloor != toFloor)
        {
            await Task.Delay(1500); // Time to move one floor
            currentFloor += toFloor > fromFloor ? 1 : -1;
            System.Console.WriteLine($"🏢 Passing floor {currentFloor}... ({Math.Abs(toFloor - currentFloor)} floors to go)");
        }
        
        System.Console.WriteLine($"🎯 Arrived at floor {toFloor}!");
        await Task.Delay(500);
    }

    private async Task ProcessRequestWithConsoleExperienceAsync(Request request)
    {
        // Find best elevator
        var elevators = _simulation.GetElevators();
        var bestElevator = _simulation.GetClosestElevator(request.Floor);
        
        if (bestElevator == null)
        {
            System.Console.WriteLine("❌ No available elevator found for this request!");
            return;
        }
        
        System.Console.WriteLine($"🎯 Dispatching Elevator {bestElevator.Id} for request from floor {request.FromFloor} to floor {request.Floor}");
        await Task.Delay(500);
        
        // Move to pickup floor if needed
        if (bestElevator.CurrentFloor != request.FromFloor)
        {
            await SimulateMovementAsync(bestElevator.Id, bestElevator.CurrentFloor, request.FromFloor);
        }
        
        // Simulate doors opening for boarding
        System.Console.WriteLine($"🚪 Elevator {bestElevator.Id} doors opening for boarding...");
        await Task.Delay(1000);
        
        // Simulate passengers boarding one by one
        for (int i = 0; i < request.PassengerCount; i++)
        {
            System.Console.WriteLine($"👤 Passenger {i + 1} boarding elevator {bestElevator.Id}... ({bestElevator.PassengerCount + i + 1}/{bestElevator.MaxCapacity})");
            await Task.Delay(800);
        }
        
        System.Console.WriteLine($"🚪 Elevator {bestElevator.Id} doors closing...");
        await Task.Delay(1000);
        
        // Move to destination floor
        await SimulateMovementAsync(bestElevator.Id, request.FromFloor, request.Floor);
        
        // Simulate doors opening for exit
        System.Console.WriteLine($"🚪 Elevator {bestElevator.Id} doors opening for exit...");
        await Task.Delay(1000);
        
        // Simulate passengers exiting
        for (int i = 0; i < request.PassengerCount; i++)
        {
            System.Console.WriteLine($"👤 Passenger exiting elevator {bestElevator.Id}... ({request.PassengerCount - i - 1} remaining)");
            await Task.Delay(600);
        }
        
        System.Console.WriteLine($"🚪 Elevator {bestElevator.Id} doors closing...");
        await Task.Delay(1000);
        
        // Actually update the elevator state to match the simulation
        await UpdateElevatorStateAsync(bestElevator, request);
        
        System.Console.WriteLine($"✅ Request completed! Elevator {bestElevator.Id} successfully delivered {request.PassengerCount} passengers from floor {request.FromFloor} to floor {request.Floor}");
    }

    private async Task UpdateElevatorStateAsync(Elevator elevator, Request request)
    {
        // Update elevator position to final destination
        elevator.CurrentFloor = request.ToFloor;
        elevator.PassengerCount = 0; // Passengers have exited
        elevator.State = ElevatorState.Idle;
        elevator.Direction = ElevatorDirection.Idle;
        
        // Clear any remaining destinations
        elevator.Destinations.Clear();
        
        // Small delay to ensure state is updated
        await Task.Delay(100);
    }

    private async Task ProcessAnyQueuedRequestsAsync()
    {
        var queue = _simulation.GetRequestQueue();
        if (queue.HasPendingRequests && _simulation.HasAvailableElevator())
        {
            System.Console.WriteLine($"\n🔄 Processing queued requests... ({queue.PendingCount} in queue)");
            await _simulation.ProcessQueuedRequestsAsync();
            
            var remainingRequests = queue.PendingCount;
            if (remainingRequests == 0)
            {
                System.Console.WriteLine("✅ All queued requests have been processed!");
            }
            else
            {
                System.Console.WriteLine($"⏳ {remainingRequests} request(s) still waiting in queue.");
            }
        }
    }

    private (int fromFloor, int toFloor, int passengers) GetElevatorRequestInput()
    {
        System.Console.WriteLine("\n📍 ELEVATOR REQUEST");
        System.Console.WriteLine("─────────────────────");
        
        // Get From Floor (with shorthand detection)
        int fromFloor;
        while (true)
        {
            System.Console.Write("From Floor (1-20) [Hint: Try floors 1, 5, 10, 15 for variety, or '1 8 5' for quick input]: ");
            var fromInput = System.Console.ReadLine()?.Trim();
            
            // Check if user entered shorthand format like "1 8 5"
            if (!string.IsNullOrEmpty(fromInput) && fromInput.Contains(' '))
            {
                try
                {
                    return ParseShorthandInput(fromInput);
                }
                catch (ArgumentException ex)
                {
                    System.Console.WriteLine($"❌ {ex.Message}");
                    System.Console.WriteLine("💡 Quick format: '[from] [to] [passengers]' or just enter single floor number");
                    continue;
                }
            }
            
            if (int.TryParse(fromInput, out fromFloor) && fromFloor >= 1 && fromFloor <= 20)
                break;
            System.Console.WriteLine("❌ Invalid floor. Please enter a number between 1 and 20.");
            System.Console.WriteLine("💡 Hint: Popular floors are 1 (lobby), 5, 10, 15, 20 (top floor)");
            System.Console.WriteLine("💡 Or use quick format: '1 8 5' (from 1 to 8, 5 passengers)");
        }
        
        // Get To Floor
        int toFloor;
        var direction = "";
        while (true)
        {
            System.Console.Write($"To Floor (1-20) [Current: {fromFloor}, Hint: Try {(fromFloor < 10 ? "15-20 for UP" : "1-5 for DOWN")}]: ");
            var toInput = System.Console.ReadLine()?.Trim();
            
            // Check if user entered shorthand format here too
            if (!string.IsNullOrEmpty(toInput) && toInput.Contains(' '))
            {
                System.Console.WriteLine("💡 Quick format detected! Use it at the main prompt instead.");
                System.Console.WriteLine("   For now, just enter the destination floor number.");
                continue;
            }
            
            if (int.TryParse(toInput, out toFloor) && toFloor >= 1 && toFloor <= 20 && toFloor != fromFloor)
            {
                direction = toFloor > fromFloor ? "UP ⬆️" : "DOWN ⬇️";
                break;
            }
            if (toFloor == fromFloor)
                System.Console.WriteLine("❌ Destination floor must be different from starting floor.");
            else
                System.Console.WriteLine("❌ Invalid floor. Please enter a number between 1 and 20.");
            System.Console.WriteLine($"💡 Hint: From floor {fromFloor}, try going {(fromFloor < 10 ? "UP to floors 15-20" : "DOWN to floors 1-5")}");
        }
        
        // Get Number of Passengers
        int passengers;
        while (true)
        {
            System.Console.Write("Number of Passengers [Hint: 1-8=Single, 9-20=Multi, 25+=Queue]: ");
            var passengersInput = System.Console.ReadLine()?.Trim();
            if (int.TryParse(passengersInput, out passengers) && passengers > 0)
                break;
            System.Console.WriteLine("❌ Invalid number. Please enter a positive number.");
            System.Console.WriteLine("💡 Capacity hints:");
            System.Console.WriteLine("   • 1-8 passengers: Single elevator (Standard)");
            System.Console.WriteLine("   • 9-20 passengers: Multi-elevator dispatch");
            System.Console.WriteLine("   • 25+ passengers: Some will be queued");
            System.Console.WriteLine("   • 50+ passengers: Multiple trips required");
        }
        
        // Confirm the request
        var totalCapacity = 48; // 8+8+12+20 = 48 max capacity
        System.Console.WriteLine($"\n✅ Request Summary:");
        System.Console.WriteLine($"   📍 From Floor {fromFloor} → To Floor {toFloor} ({direction})");
        System.Console.WriteLine($"   👥 Passengers: {passengers}");
        System.Console.WriteLine($"   🏢 Building Capacity: {totalCapacity} passengers total (4 elevators)");
        System.Console.WriteLine($"   🚀 Strategy: {(passengers > totalCapacity ? "Multi-trip required" : passengers > 20 ? "Multi-elevator dispatch" : "Single elevator")}");
        
        return (fromFloor, toFloor, passengers);
    }

    private (int fromFloor, int toFloor, int passengers) ParseShorthandInput(string input)
    {
        // Check if input contains comma - indicating multi-destination format
        if (input.Contains(','))
        {
            throw new ArgumentException("Multi-destination format detected. Use: [3 8 12, 7 8 12] - will be processed as separate requests");
        }
        
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 2 && int.TryParse(parts[0], out var toFloor) && int.TryParse(parts[1], out var passengers))
        {
            // Format: [to_floor] [passengers] (from floor 1)
            ValidateInput(1, toFloor, passengers);
            return (1, toFloor, passengers);
        }
        else if (parts.Length == 3 && int.TryParse(parts[0], out var fromFloor) && 
                 int.TryParse(parts[1], out var toFloor2) && int.TryParse(parts[2], out var passengers2))
        {
            // Format: [from_floor] [to_floor] [passengers]
            ValidateInput(fromFloor, toFloor2, passengers2);
            return (fromFloor, toFloor2, passengers2);
        }
        else if (parts.Length == 1 && int.TryParse(parts[0], out var toFloor3))
        {
            // Format: [to_floor] (1 passenger from floor 1)
            ValidateInput(1, toFloor3, 1);
            return (1, toFloor3, 1);
        }
        else
        {
            throw new ArgumentException($"Invalid format. Use: [to_floor] [passengers] or [from_floor] [to_floor] [passengers] or just [to_floor]");
        }
    }
    
    private List<Request> ParseMultiDestinationInput(string input)
    {
        var requests = new List<Request>();
        
        // Remove square brackets if present
        var cleanInput = input.Trim().TrimStart('[').TrimEnd(']');
        var tripStrings = cleanInput.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var tripString in tripStrings)
        {
            // Clean each trip string and remove any remaining brackets
            var cleanTripString = tripString.Trim().TrimStart('[').TrimEnd(']');
            var parts = cleanTripString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 3 && 
                int.TryParse(parts[0], out var fromFloor) && 
                int.TryParse(parts[1], out var toFloor) && 
                int.TryParse(parts[2], out var passengers))
            {
                ValidateInput(fromFloor, toFloor, passengers);
                requests.Add(new Request(fromFloor, toFloor, passengers));
            }
            else
            {
                throw new ArgumentException($"Invalid trip format: '{cleanTripString}'. Each trip must be: [from_floor] [to_floor] [passengers]");
            }
        }
        
        return requests;
    }

    private void ValidateInput(int fromFloor, int toFloor, int passengers)
    {
        if (fromFloor < 1 || fromFloor > 20)
            throw new ArgumentException("From floor must be between 1 and 20");
        if (toFloor < 1 || toFloor > 20)
            throw new ArgumentException("To floor must be between 1 and 20");
        if (fromFloor == toFloor)
            throw new ArgumentException("From floor and to floor must be different");
        if (passengers < 1)
            throw new ArgumentException("Number of passengers must be positive");
    }

    private async Task ProcessBatchTripsAsync()
    {
        var batchRequests = new List<Request>();
        
        System.Console.Clear();
        System.Console.WriteLine("🎭 BATCH TRIP SIMULATION");
        System.Console.WriteLine("═══════════════════════════");
        System.Console.WriteLine("📝 Add as many trips as you want, then start simulation");
        System.Console.WriteLine("💡 System will intelligently queue and dispatch all trips");
        System.Console.WriteLine("🏢 4 Elevators Available: Standard(8), Standard(8), High-Speed(12), Freight(20)");
        System.Console.WriteLine("💭 Suggested scenarios to try:");
        System.Console.WriteLine("   • Rush hour: Multiple trips from floor 1 going UP");
        System.Console.WriteLine("   • Lunch time: Mix of UP and DOWN trips");
        System.Console.WriteLine("   • Heavy load: 30+ passengers to test queueing");
        System.Console.WriteLine();
        
        while (true)
        {
            System.Console.WriteLine($"📊 Current batch: {batchRequests.Count} trip(s) added");
            if (batchRequests.Any())
            {
                System.Console.WriteLine("📋 Trips in batch:");
                for (int i = 0; i < batchRequests.Count; i++)
                {
                    var req = batchRequests[i];
                    var dir = req.Direction == Domain.Enums.ElevatorDirection.Up ? "UP" : "DOWN";
                    System.Console.WriteLine($"   {i + 1}. Floor {req.FromFloor} → {req.ToFloor} ({dir}) - {req.PassengerCount} passengers");
                }
                System.Console.WriteLine();
            }
            
            System.Console.WriteLine("🚪 Options:");
            System.Console.WriteLine("   • Press ENTER to add another trip");
            System.Console.WriteLine("   • Type 'start' to begin simulation");
            System.Console.WriteLine("   • Type 'clear' to clear all trips");
            System.Console.WriteLine("   • Type 'back' to return to main menu");
            System.Console.Write("\nChoice: ");
            
            var choice = System.Console.ReadLine()?.Trim().ToLower();
            
            if (choice == "back") return;
            
            if (choice == "clear")
            {
                batchRequests.Clear();
                System.Console.WriteLine("✅ All trips cleared!");
                continue;
            }
            
            if (choice == "start")
            {
                if (!batchRequests.Any())
                {
                    System.Console.WriteLine("❌ No trips to simulate. Add some trips first!");
                    System.Console.WriteLine("Press Enter to continue...");
                    System.Console.ReadLine();
                    continue;
                }
                
                await StartBatchSimulation(batchRequests);
                return;
            }
            
            // Add new trip (ENTER or any other input)
            try
            {
                System.Console.WriteLine($"\n📍 ADDING TRIP #{batchRequests.Count + 1}");
                System.Console.WriteLine("─────────────────────────");
                System.Console.WriteLine("💡 Pro tip: Mix different floors and passenger counts for realistic simulation!");
                var (fromFloor, toFloor, passengers) = GetElevatorRequestInput();
                
                var newRequest = new Request(fromFloor, toFloor, passengers);
                batchRequests.Add(newRequest);
                
                System.Console.WriteLine($"✅ Trip #{batchRequests.Count} added successfully!");
                System.Console.WriteLine("Press Enter to continue...");
                System.Console.ReadLine();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error adding trip: {ex.Message}");
                System.Console.WriteLine("Press Enter to continue...");
                System.Console.ReadLine();
            }
        }
    }

    private async Task StartBatchSimulation(List<Request> requests)
    {
        var startTime = DateTime.Now;
        var logger = (_simulation as SimulationService)?.GetLogger();
        var totalPassengers = requests.Sum(r => r.PassengerCount);
        
        System.Console.Clear();
        System.Console.WriteLine("🎬 STARTING BATCH SIMULATION");
        System.Console.WriteLine("═══════════════════════════════");
        System.Console.WriteLine($"📊 Total trips to simulate: {requests.Count}");
        System.Console.WriteLine($"👥 Total passengers: {totalPassengers}");
        System.Console.WriteLine();
        
        // Log batch operation start
        logger?.LogBatchOperation("STARTED", requests.Count, totalPassengers);
        
        // Show trip summary
        System.Console.WriteLine("📋 TRIP SUMMARY:");
        for (int i = 0; i < requests.Count; i++)
        {
            var req = requests[i];
            var dir = req.Direction == Domain.Enums.ElevatorDirection.Up ? "UP" : "DOWN";
            System.Console.WriteLine($"   {i + 1}. Floor {req.FromFloor} → {req.ToFloor} ({dir}) - {req.PassengerCount} passengers");
        }
        
        System.Console.WriteLine();
        System.Console.WriteLine("🚀 Press ENTER to start simulation or type 'back' to modify trips...");
        var confirm = System.Console.ReadLine()?.Trim().ToLower();
        
        if (confirm == "back") return;
        
        System.Console.WriteLine("\n🎯 SIMULATION STARTING...");
        System.Console.WriteLine("=" + new string('=', 80));
        
        // Process all requests with intelligent queueing
        var queuedRequests = new List<Request>();
        var completedTrips = 0;
        
        for (int i = 0; i < requests.Count; i++)
        {
            var request = requests[i];
            System.Console.WriteLine($"\n🔄 Processing Trip #{i + 1}: {request.PassengerCount} passengers from floor {request.FromFloor} to floor {request.ToFloor}");
            
            // Log each batch request
            logger?.LogRequestStart(request, "Batch");
            
            if (!_simulation.HasAvailableElevator())
            {
                System.Console.WriteLine($"⏳ All elevators busy! Queueing trip #{i + 1}");
                queuedRequests.Add(request);
                var queue = _simulation.GetRequestQueue();
                logger?.LogQueueOperation("ADDED", queue.PendingCount + 1, request);
                await _simulation.ProcessRequestAsync(request); // This will queue it
            }
            else
            {
                var remainingPassengers = await _simulation.ProcessRequestAsync(request);
                if (remainingPassengers > 0)
                {
                    var queuedRequest = new Request(request.FromFloor, request.ToFloor, remainingPassengers);
                    queuedRequests.Add(queuedRequest);
                    logger?.LogCapacityOverflow(request.PassengerCount, request.PassengerCount - remainingPassengers, remainingPassengers);
                    System.Console.WriteLine($"⏳ {remainingPassengers} passengers from trip #{i + 1} queued due to capacity limits");
                }
                completedTrips++;
            }
            
            // Small delay between requests for realism
            await Task.Delay(1000);
        }
        
        // Process queued requests
        if (queuedRequests.Any())
        {
            System.Console.WriteLine($"\n🔄 Processing {queuedRequests.Count} queued request(s)...");
            logger?.LogBatchOperation("PROCESSING_QUEUE", queuedRequests.Count, queuedRequests.Sum(r => r.PassengerCount));
            await _simulation.ProcessQueuedRequestsAsync();
        }
        
        var duration = DateTime.Now - startTime;
        
        // Log final statistics
        logger?.LogBatchOperation("COMPLETED", requests.Count, totalPassengers);
        logger?.LogSystemStats(requests.Count, totalPassengers, duration);
        
        System.Console.WriteLine("\nBATCH SIMULATION COMPLETED!");
        System.Console.WriteLine("=" + new string('=', 80));
        System.Console.WriteLine($"Successfully processed {requests.Count} trips");
        System.Console.WriteLine($"Total passengers transported: {totalPassengers}");
        System.Console.WriteLine($"Total simulation time: {duration:mm\\:ss}");
        System.Console.WriteLine($"Average time per trip: {duration.TotalSeconds / requests.Count:F1} seconds");
        
        // Show comprehensive simulation summary
        ShowSimulationSummary(requests);
        
        // Show log summary
        var logSummary = logger?.GetLogSummary();
        if (logSummary != null)
        {
            System.Console.WriteLine($"\nLOG SUMMARY:");
            System.Console.WriteLine($"   • Total log entries: {logSummary.TotalEntries}");
            System.Console.WriteLine($"   • Total requests: {logSummary.TotalRequests}");
            System.Console.WriteLine($"   • Total trips: {logSummary.TotalTrips}");
            System.Console.WriteLine($"   • Warnings: {logSummary.WarningCount}");
            System.Console.WriteLine($"   • Errors: {logSummary.ErrorCount}");
        }
        
        System.Console.WriteLine("\nPress Enter to continue...");
        System.Console.ReadLine();
    }

    private void ShowComprehensiveLogs()
    {
        var elevatorLogger = (_simulation as SimulationService)?.GetLogger();
        if (elevatorLogger == null)
        {
            System.Console.WriteLine("❌ Logger not available");
            System.Console.WriteLine("Press Enter to continue...");
            System.Console.ReadLine();
            return;
        }

        System.Console.Clear();
        System.Console.WriteLine("📊 COMPREHENSIVE SYSTEM LOGS");
        System.Console.WriteLine("════════════════════════════════");
        
        var logSummary = elevatorLogger.GetLogSummary();
        
        // Show log summary
        System.Console.WriteLine("📋 LOG SUMMARY:");
        System.Console.WriteLine($"   • Total entries: {logSummary.TotalEntries}");
        System.Console.WriteLine($"   • Total requests: {logSummary.TotalRequests}");
        System.Console.WriteLine($"   • Total trips: {logSummary.TotalTrips}");
        System.Console.WriteLine($"   • Warnings: {logSummary.WarningCount}");
        System.Console.WriteLine($"   • Errors: {logSummary.ErrorCount}");
        System.Console.WriteLine($"   • Session start: {logSummary.StartTime:HH:mm:ss}");
        System.Console.WriteLine($"   • Last activity: {logSummary.LastActivity:HH:mm:ss}");
        System.Console.WriteLine();
        
        while (true)
        {
            System.Console.WriteLine("🔍 LOG VIEWER OPTIONS:");
            System.Console.WriteLine("   1. Recent Activity (last 20 entries)");
            System.Console.WriteLine("   2. All Requests");
            System.Console.WriteLine("   3. All Trips");
            System.Console.WriteLine("   4. Queue Operations");
            System.Console.WriteLine("   5. Capacity Issues");
            System.Console.WriteLine("   6. Movement Logs");
            System.Console.WriteLine("   7. System Statistics");
            System.Console.WriteLine("   8. All Logs (full history)");
            System.Console.WriteLine("   9. Clear All Logs");
            System.Console.WriteLine("   0. Back to Main Menu");
            System.Console.WriteLine();
            System.Console.Write("Choose option (0-9): ");
            
            var choice = System.Console.ReadLine()?.Trim();
            
            switch (choice)
            {
                case "1":
                    ShowRecentLogs(elevatorLogger);
                    break;
                case "2":
                    ShowLogsByCategory(elevatorLogger, LogCategory.Request, "REQUESTS");
                    break;
                case "3":
                    ShowLogsByCategory(elevatorLogger, LogCategory.Trip, "TRIPS");
                    break;
                case "4":
                    ShowLogsByCategory(elevatorLogger, LogCategory.Queue, "QUEUE OPERATIONS");
                    break;
                case "5":
                    ShowLogsByCategory(elevatorLogger, LogCategory.Capacity, "CAPACITY ISSUES");
                    break;
                case "6":
                    ShowLogsByCategory(elevatorLogger, LogCategory.Movement, "MOVEMENT LOGS");
                    break;
                case "7":
                    ShowLogsByCategory(elevatorLogger, LogCategory.Statistics, "STATISTICS");
                    break;
                case "8":
                    ShowAllLogs(elevatorLogger);
                    break;
                case "9":
                    elevatorLogger.ClearLogs();
                    System.Console.WriteLine("✅ All logs cleared!");
                    System.Console.WriteLine("Press Enter to continue...");
                    System.Console.ReadLine();
                    break;
                case "0":
                    return;
                default:
                    System.Console.WriteLine("❌ Invalid option. Please try again.");
                    System.Console.WriteLine("Press Enter to continue...");
                    System.Console.ReadLine();
                    break;
            }
            
            System.Console.Clear();
            System.Console.WriteLine("📊 COMPREHENSIVE SYSTEM LOGS");
            System.Console.WriteLine("════════════════════════════════");
        }
    }

    private void ShowRecentLogs(ElevatorLogger elevatorLogger)
    {
        System.Console.Clear();
        System.Console.WriteLine("📝 RECENT ACTIVITY (Last 20 entries)");
        System.Console.WriteLine("═══════════════════════════════════");
        
        var recentLogs = elevatorLogger.GetRecentLogs(20);
        if (!recentLogs.Any())
        {
            System.Console.WriteLine("No recent activity found.");
        }
        else
        {
            foreach (var log in recentLogs)
            {
                System.Console.WriteLine(log);
            }
        }
        
        System.Console.WriteLine();
        System.Console.WriteLine("Press Enter to return to log menu...");
        System.Console.ReadLine();
    }

    private void ShowLogsByCategory(ElevatorLogger elevatorLogger, LogCategory category, string categoryName)
    {
        System.Console.Clear();
        System.Console.WriteLine($"📂 {categoryName}");
        System.Console.WriteLine("═".PadRight(categoryName.Length + 5, '═'));
        
        var logs = elevatorLogger.GetLogsByCategory(category, 50);
        if (!logs.Any())
        {
            System.Console.WriteLine($"No {categoryName.ToLower()} found.");
        }
        else
        {
            foreach (var log in logs)
            {
                System.Console.WriteLine(log.FormattedMessage);
            }
        }
        
        System.Console.WriteLine();
        System.Console.WriteLine($"📊 Total {categoryName.ToLower()}: {logs.Count}");
        System.Console.WriteLine("Press Enter to return to log menu...");
        System.Console.ReadLine();
    }

    private void ShowAllLogs(ElevatorLogger elevatorLogger)
    {
        System.Console.Clear();
        System.Console.WriteLine("📖 ALL LOGS (Complete History)");
        System.Console.WriteLine("════════════════════════════════");
        
        var allLogs = elevatorLogger.GetAllLogs();
        if (!allLogs.Any())
        {
            System.Console.WriteLine("No logs found.");
        }
        else
        {
            System.Console.WriteLine($"📊 Showing {allLogs.Count} log entries:");
            System.Console.WriteLine();
            
            foreach (var log in allLogs)
            {
                System.Console.WriteLine(log.FormattedMessage);
            }
        }
        
        System.Console.WriteLine();
        System.Console.WriteLine("Press Enter to return to log menu...");
        System.Console.ReadLine();
    }

    private void ShowSimulationSummary(List<Request> requests)
    {
        var elevators = _simulation.GetElevators();
        var logger = (_simulation as SimulationService)?.GetLogger();
        var queue = _simulation.GetRequestQueue();
        
        System.Console.WriteLine("\n=== SIMULATION SUMMARY ===");
        
        // Trip Details with Elevator Information
        System.Console.WriteLine("TRIP DETAILS:");
        var totalPassengers = requests.Sum(r => r.PassengerCount);
        var totalDistance = 0;
        var elevatorUsage = new Dictionary<int, int>(); // Track which elevators were used
        
        for (int i = 0; i < requests.Count; i++)
        {
            var request = requests[i];
            var distance = Math.Abs(request.ToFloor - request.FromFloor);
            totalDistance += distance;
            var direction = request.Direction == Domain.Enums.ElevatorDirection.Up ? "UP" : "DOWN";
            
            // Determine which elevators were likely used based on capacity and current positions
            var elevatorsUsed = GetElevatorsUsedForRequest(request, elevators);
            var elevatorDetails = GetElevatorDetailsForRequest(request, elevatorsUsed);
            
            System.Console.WriteLine($"  Trip {i + 1}: {request.PassengerCount} passengers, Floor {request.FromFloor} to {request.ToFloor} ({direction}), Distance: {distance} floors");
            System.Console.WriteLine($"           Served by: {elevatorDetails}");
            
            // Track elevator usage
            foreach (var elevator in elevatorsUsed)
            {
                elevatorUsage[elevator.Id] = elevatorUsage.GetValueOrDefault(elevator.Id, 0) + 1;
            }
        }
        
        System.Console.WriteLine();
        
        // System Statistics
        System.Console.WriteLine("SYSTEM STATISTICS:");
        System.Console.WriteLine($"  Total Trips: {requests.Count}");
        System.Console.WriteLine($"  Total Passengers: {totalPassengers}");
        System.Console.WriteLine($"  Total Distance: {totalDistance} floors");
        System.Console.WriteLine($"  Average Distance per Trip: {(requests.Count > 0 ? (double)totalDistance / requests.Count : 0):F1} floors");
        System.Console.WriteLine($"  Average Passengers per Trip: {(requests.Count > 0 ? (double)totalPassengers / requests.Count : 0):F1}");
        
        // Elevator Usage Statistics
        if (elevatorUsage.Any())
        {
            System.Console.WriteLine();
            System.Console.WriteLine("ELEVATOR USAGE STATISTICS:");
            foreach (var usage in elevatorUsage.OrderBy(kvp => kvp.Key))
            {
                var elevator = elevators.First(e => e.Id == usage.Key);
                var usagePercentage = requests.Count > 0 ? (double)usage.Value / requests.Count * 100 : 0;
                System.Console.WriteLine($"  Elevator {usage.Key} ({GetElevatorTypeDescription(elevator)}): Used in {usage.Value}/{requests.Count} trips ({usagePercentage:F1}%)");
            }
            
            var mostUsedElevator = elevatorUsage.OrderByDescending(kvp => kvp.Value).First();
            var mostUsedElevatorInfo = elevators.First(e => e.Id == mostUsedElevator.Key);
            System.Console.WriteLine($"  Most Active: Elevator {mostUsedElevator.Key} ({GetElevatorTypeDescription(mostUsedElevatorInfo)}) - {mostUsedElevator.Value} trips");
        }
        
        // Elevator Status
        System.Console.WriteLine();
        System.Console.WriteLine("CURRENT ELEVATOR STATUS:");
        var activeElevators = 0;
        var totalCurrentPassengers = 0;
        var totalCapacityUsed = 0;
        var totalMaxCapacity = elevators.Sum(e => e.MaxCapacity);
        
        foreach (var elevator in elevators)
        {
            var status = elevator.State == Domain.Enums.ElevatorState.Idle ? "IDLE" : "BUSY";
            var utilization = elevator.MaxCapacity > 0 ? (double)elevator.PassengerCount / elevator.MaxCapacity * 100 : 0;
            
            System.Console.WriteLine($"  Elevator {elevator.Id} ({GetElevatorTypeDescription(elevator)}): Floor {elevator.CurrentFloor}, {status}, " +
                                   $"{elevator.PassengerCount}/{elevator.MaxCapacity} passengers ({utilization:F0}% capacity)");
            
            if (elevator.State != Domain.Enums.ElevatorState.Idle)
                activeElevators++;
            
            totalCurrentPassengers += elevator.PassengerCount;
            totalCapacityUsed += elevator.PassengerCount;
        }
        
        // System Efficiency
        System.Console.WriteLine();
        System.Console.WriteLine("SYSTEM EFFICIENCY:");
        var overallUtilization = totalMaxCapacity > 0 ? (double)totalCapacityUsed / totalMaxCapacity * 100 : 0;
        System.Console.WriteLine($"  Elevators Active: {activeElevators}/4 ({(double)activeElevators / 4 * 100:F0}%)");
        System.Console.WriteLine($"  Current Passengers in System: {totalCurrentPassengers}");
        System.Console.WriteLine($"  Overall Capacity Utilization: {overallUtilization:F1}%");
        System.Console.WriteLine($"  Queue Status: {(queue.HasPendingRequests ? $"{queue.PendingCount} requests pending" : "Empty")}");
        
        // Performance Metrics
        if (logger != null)
        {
            var logSummary = logger.GetLogSummary();
            System.Console.WriteLine();
            System.Console.WriteLine("PERFORMANCE METRICS:");
            System.Console.WriteLine($"  Total System Requests: {logSummary.TotalRequests}");
            System.Console.WriteLine($"  Total System Trips: {logSummary.TotalTrips}");
            System.Console.WriteLine($"  System Warnings: {logSummary.WarningCount}");
            System.Console.WriteLine($"  System Errors: {logSummary.ErrorCount}");
            
            if (logSummary.TotalRequests > 0)
            {
                var successRate = ((double)(logSummary.TotalRequests - logSummary.ErrorCount) / logSummary.TotalRequests) * 100;
                System.Console.WriteLine($"  Success Rate: {successRate:F1}%");
            }
        }
        
        // Capacity Analysis
        System.Console.WriteLine();
        System.Console.WriteLine("CAPACITY ANALYSIS:");
        var excessPassengers = Math.Max(0, totalPassengers - totalMaxCapacity);
        if (excessPassengers > 0)
        {
            System.Console.WriteLine($"  NOTICE: {excessPassengers} passengers exceeded building capacity");
            System.Console.WriteLine($"  These passengers were queued for subsequent trips");
        }
        else
        {
            var remainingCapacity = totalMaxCapacity - totalPassengers;
            System.Console.WriteLine($"  Building handled all passengers efficiently");
            System.Console.WriteLine($"  Remaining unused capacity: {remainingCapacity} passengers");
        }
        
        System.Console.WriteLine("========================");
    }
    
    private List<Elevator> GetElevatorsUsedForRequest(Request request, List<Elevator> elevators)
    {
        var elevatorsUsed = new List<Elevator>();
        var remainingPassengers = request.PassengerCount;
        
        // Enhanced dispatch logic matching the actual DispatchService behavior
        var availableElevators = elevators
            .Where(e => e.State == Domain.Enums.ElevatorState.Idle && e.PassengerCount < e.MaxCapacity)
            .OrderBy(e => IsElevatorInWrongDirection(e, request) ? 1 : 0) // Same direction first
            .ThenBy(e => Math.Abs(e.CurrentFloor - request.FromFloor)) // Closest to pickup
            .ThenByDescending(e => e.MaxCapacity - e.PassengerCount) // Most available capacity
            .ThenBy(e => e.Id) // Tie-breaker
            .ToList();
        
        foreach (var elevator in availableElevators)
        {
            if (remainingPassengers <= 0) break;
            
            var availableCapacity = elevator.MaxCapacity - elevator.PassengerCount;
            var canTake = Math.Min(remainingPassengers, availableCapacity);
            
            if (canTake > 0)
            {
                elevatorsUsed.Add(elevator);
                remainingPassengers -= canTake;
            }
        }
        
        // If no elevators were available, try to find the best single elevator
        if (!elevatorsUsed.Any())
        {
            var bestElevator = elevators
                .Where(e => e.State == Domain.Enums.ElevatorState.Idle)
                .OrderBy(e => Math.Abs(e.CurrentFloor - request.FromFloor))
                .ThenBy(e => e.Id)
                .FirstOrDefault();
                
            if (bestElevator != null)
            {
                elevatorsUsed.Add(bestElevator);
            }
        }
        
        return elevatorsUsed;
    }
    
    private bool IsElevatorInWrongDirection(Elevator elevator, Request request)
    {
        // If elevator is idle, no direction preference
        if (elevator.Direction == Domain.Enums.ElevatorDirection.Idle) return false;
        
        // Check if elevator direction matches request direction
        return elevator.Direction != request.Direction;
    }
    
    private string GetElevatorDetailsForRequest(Request request, List<Elevator> elevatorsUsed)
    {
        if (!elevatorsUsed.Any())
        {
            return "None available";
        }
        
        if (elevatorsUsed.Count == 1)
        {
            var elevator = elevatorsUsed[0];
            var distance = Math.Abs(elevator.CurrentFloor - request.FromFloor);
            return $"Elevator {elevator.Id} ({GetElevatorTypeDescription(elevator)}) - {distance} floors away, capacity {elevator.MaxCapacity}";
        }
        
        // Multiple elevators - show passenger distribution
        var details = new List<string>();
        var remainingPassengers = request.PassengerCount;
        
        foreach (var elevator in elevatorsUsed)
        {
            var availableCapacity = elevator.MaxCapacity - elevator.PassengerCount;
            var canTake = Math.Min(remainingPassengers, availableCapacity);
            var distance = Math.Abs(elevator.CurrentFloor - request.FromFloor);
            
            details.Add($"Elevator {elevator.Id} ({GetElevatorTypeDescription(elevator)}) - {canTake} passengers, {distance} floors away");
            remainingPassengers -= canTake;
            
            if (remainingPassengers <= 0) break;
        }
        
        return string.Join(" + ", details);
    }
    
    private string GetElevatorTypeDescription(Elevator elevator)
    {
        return elevator.Type switch
        {
            Domain.Enums.ElevatorType.Standard => "Standard",
            Domain.Enums.ElevatorType.HighSpeed => "High-Speed",
            Domain.Enums.ElevatorType.Freight => "Freight",
            _ => "Unknown"
        };
    }

    private void ShowGoodbyeScreen()
    {
        System.Console.Clear();
        System.Console.WriteLine("=== GOODBYE ===");
        System.Console.WriteLine("Thank you for using DVT Elevator Simulator!");
        System.Console.WriteLine("Press Enter to exit...");
        System.Console.ReadLine();
    }
}