using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Interfaces;

namespace ElevatorSimulator.Application.Services;

/// <summary>
/// Service for dispatching elevators following DIP
/// </summary>
public class DispatchService : IDispatchService
{
    private readonly IElevatorService _elevatorService;

    public DispatchService(IElevatorService elevatorService)
    {
        _elevatorService = elevatorService;
    }

    public Elevator? FindBestElevator(List<Elevator> elevators, Request request)
    {
        var available = elevators
            .Where(e => e.State != ElevatorState.OutOfService && 
                       e.State == ElevatorState.Idle &&
                       _elevatorService.CanAcceptPassengers(e, request.PassengerCount))
            .ToList();

        if (!available.Any()) return null;

        return available
            .OrderBy(e => _elevatorService.CalculateDistance(e, request.FromFloor))
            .ThenBy(e => e.Id) // Tie-breaker for same distance
            .First();
    }

    public Elevator? FindClosestElevator(List<Elevator> elevators, int targetFloor)
    {
        return elevators
            .Where(e => e.State != ElevatorState.OutOfService)
            .OrderBy(e => _elevatorService.CalculateDistance(e, targetFloor))
            .FirstOrDefault();
    }

    public async Task<int> ProcessRequestAsync(List<Elevator> elevators, Request request)
    {
        var subRequests = DistributePassengers(elevators, request);
        
        if (!subRequests.Any())
        {
            Console.WriteLine("No elevators available to handle this request");
            return request.PassengerCount; // All passengers need to be queued
        }
        
        // Calculate total passengers that can be accommodated
        var accommodatedPassengers = subRequests.Sum(sr => sr.Item2);
        var remainingPassengers = request.PassengerCount - accommodatedPassengers;
        
        Console.WriteLine($"\nStarting multi-elevator operation:");
        Console.WriteLine($"   Accommodating: {accommodatedPassengers} passengers across {subRequests.Count} elevator(s)");
        if (remainingPassengers > 0)
        {
            Console.WriteLine($"   Queueing: {remainingPassengers} passengers for next available elevator");
        }
        Console.WriteLine();
        
        // Process all sub-requests in parallel with detailed activity logging
        var tasks = subRequests.Select(async subRequest =>
        {
            var elevator = subRequest.Item1;
            var passengerCount = subRequest.Item2;
            var elevatorType = GetElevatorTypeDescription(elevator);
            
            Console.WriteLine($"\nDISPATCHING ELEVATOR {elevator.Id} ({elevatorType})");
            Console.WriteLine($"   Mission: Transport {passengerCount} passenger(s) from floor {request.FromFloor} to floor {request.ToFloor}");
            Console.WriteLine($"   Current position: Floor {elevator.CurrentFloor}");
            await Task.Delay(800);
            
            // First move elevator to pickup floor if not already there
            if (elevator.CurrentFloor != request.FromFloor)
            {
                Console.WriteLine($"\nPHASE 1: Moving to pickup floor {request.FromFloor}");
                await SimulateDetailedMovement(elevator, elevator.CurrentFloor, request.FromFloor);
                await _elevatorService.MoveToFloorAsync(elevator, request.FromFloor);
            }
            else
            {
                Console.WriteLine($"\nAlready at pickup floor {request.FromFloor}");
                await Task.Delay(500);
            }
            
            // Load passengers at pickup floor
            Console.WriteLine($"\nPHASE 2: Passenger boarding at floor {request.FromFloor}");
            Console.WriteLine($"   *DING* Elevator {elevator.Id} doors opening...");
            await Task.Delay(1000);
            
            Console.WriteLine($"   {passengerCount} passenger(s) boarding Elevator {elevator.Id}");
            for (int i = 1; i <= passengerCount; i++)
            {
                Console.WriteLine($"   Passenger {i}/{passengerCount} enters elevator... ({i}/{elevator.MaxCapacity} capacity)");
                await Task.Delay(300);
            }
            
            await _elevatorService.LoadPassengersAsync(elevator, passengerCount);
            Console.WriteLine($"   Doors closing... All passengers aboard!");
            await Task.Delay(1000);
            
            // Add destination floor
            _elevatorService.AddDestination(elevator, request.ToFloor);
            Console.WriteLine($"   Destination set: Floor {request.ToFloor}");
            await Task.Delay(500);
            
            // Move to destination floor
            Console.WriteLine($"\nPHASE 3: Moving to destination floor {request.ToFloor}");
            await SimulateDetailedMovement(elevator, request.FromFloor, request.ToFloor);
            await _elevatorService.MoveToFloorAsync(elevator, request.ToFloor);
            
            // Unload passengers at destination
            Console.WriteLine($"\nPHASE 4: Passenger alighting at floor {request.ToFloor}");
            Console.WriteLine($"   *DING* Elevator {elevator.Id} doors opening...");
            await Task.Delay(1000);
            
            Console.WriteLine($"   {passengerCount} passenger(s) exiting Elevator {elevator.Id}");
            for (int i = 1; i <= passengerCount; i++)
            {
                Console.WriteLine($"   Passenger {i}/{passengerCount} exits elevator");
                await Task.Delay(300);
            }
            
            await _elevatorService.UnloadPassengersAsync(elevator, passengerCount);
            Console.WriteLine($"   Doors closing... Mission complete!");
            await Task.Delay(1000);
            
            Console.WriteLine($"\nELEVATOR {elevator.Id} MISSION COMPLETED!");
            Console.WriteLine($"   Successfully transported {passengerCount} passengers");
            Console.WriteLine($"   Final position: Floor {request.ToFloor}");
            Console.WriteLine($"   Ready for next assignment");
        });
        
        await Task.WhenAll(tasks);
        
        // Handle remaining passengers by creating queued requests
        if (remainingPassengers > 0)
        {
            Console.WriteLine($"\n{remainingPassengers} passengers will be queued for next available elevator");
            return remainingPassengers; // Return for queue handling
        }
        
        return 0; // All passengers accommodated
    }

    private List<(Elevator, int)> DistributePassengers(List<Elevator> elevators, Request request)
    {
        var availableElevators = GetAvailableElevators(elevators, request);
        var distribution = new List<(Elevator, int)>();
        var remainingPassengers = request.PassengerCount;
        
        Console.WriteLine($"Distributing {request.PassengerCount} passengers across available elevators");
        Console.WriteLine($"   Direction: {request.Direction} (Floor {request.FromFloor} to {request.ToFloor})");
        
        foreach (var elevator in availableElevators)
        {
            if (remainingPassengers <= 0) break;
            
            // Respect elevator capacity limits
            var availableCapacity = elevator.MaxCapacity - elevator.PassengerCount;
            var canTake = Math.Min(remainingPassengers, availableCapacity);
            
            if (canTake > 0)
            {
                distribution.Add((elevator, canTake));
                remainingPassengers -= canTake;
                
                var elevatorType = GetElevatorTypeDescription(elevator);
                Console.WriteLine($"   Elevator {elevator.Id} ({elevatorType}): {canTake} passengers (Available: {availableCapacity}/{elevator.MaxCapacity})");
            }
        }
        
        if (remainingPassengers > 0)
        {
            Console.WriteLine($"WARNING: {remainingPassengers} passengers will be queued (insufficient elevator capacity)");
            Console.WriteLine($"   Total building capacity: {elevators.Where(e => e.State == ElevatorState.Idle).Sum(e => e.MaxCapacity - e.PassengerCount)} passengers");
        }
        else
        {
            Console.WriteLine($"SUCCESS: All {request.PassengerCount} passengers accommodated across {distribution.Count} elevator(s)");
        }
        
        return distribution;
    }

    private string GetElevatorTypeDescription(Elevator elevator)
    {
        return elevator.Type switch
        {
            ElevatorType.Standard => "Standard",
            ElevatorType.HighSpeed => "High-Speed", 
            ElevatorType.Freight => "Freight",
            _ => "Unknown"
        };
    }

    private List<Elevator> GetAvailableElevators(List<Elevator> elevators, Request request)
    {
        // Get all idle elevators with available capacity
        var availableElevators = elevators
            .Where(e => e.State == ElevatorState.Idle && e.PassengerCount < e.MaxCapacity)
            .ToList();
        
        if (!availableElevators.Any())
        {
            Console.WriteLine("No elevators available - all are busy or at full capacity");
            return new List<Elevator>();
        }
        
        // Smart sorting for optimal passenger distribution
        var elevatorsByPreference = availableElevators
            .OrderBy(e => IsElevatorInWrongDirection(e, request) ? 1 : 0) // 1. Same direction first
            .ThenBy(e => _elevatorService.CalculateDistance(e, request.FromFloor)) // 2. Closest to pickup
            .ThenByDescending(e => e.MaxCapacity - e.PassengerCount) // 3. Most available capacity
            .ThenBy(e => e.Id) // 4. Tie-breaker
            .ToList();
        
        Console.WriteLine($"Available elevators (by preference):");
        foreach (var elevator in elevatorsByPreference.Take(3)) // Show top 3
        {
            var distance = _elevatorService.CalculateDistance(elevator, request.FromFloor);
            var capacity = elevator.MaxCapacity - elevator.PassengerCount;
            var type = GetElevatorTypeDescription(elevator);
            Console.WriteLine($"   Elevator {elevator.Id} ({type}): {distance} floors away, {capacity} capacity, Floor {elevator.CurrentFloor}");
        }
        
        return elevatorsByPreference;
    }

    private bool IsElevatorInWrongDirection(Elevator elevator, Request request)
    {
        // If elevator is idle, no direction preference
        if (elevator.Direction == ElevatorDirection.Idle) return false;
        
        // Check if elevator direction matches request direction
        return elevator.Direction != request.Direction;
    }

    private async Task SimulateDetailedMovement(Elevator elevator, int fromFloor, int toFloor)
    {
        if (fromFloor == toFloor) return;
        
        var direction = toFloor > fromFloor ? "UP" : "DOWN";
        var floorDifference = Math.Abs(toFloor - fromFloor);
        var elevatorType = GetElevatorTypeDescription(elevator);
        
        // Different elevator types have different speeds
        var baseSpeed = elevator.Type switch
        {
            ElevatorType.HighSpeed => 800,   // Faster
            ElevatorType.Standard => 1200,   // Normal
            ElevatorType.Freight => 1500,    // Slower (heavier)
            _ => 1200
        };
        
        Console.WriteLine($"   Elevator {elevator.Id} ({elevatorType}) starting {direction} journey");
        Console.WriteLine($"   Distance: {floorDifference} floors | Estimated time: {(floorDifference * baseSpeed / 1000.0):F1} seconds");
        await Task.Delay(500);
        
        var currentFloor = fromFloor;
        var step = toFloor > fromFloor ? 1 : -1;
        
        Console.WriteLine($"   Departing floor {fromFloor}...");
        await Task.Delay(baseSpeed / 2);
        
        while (currentFloor != toFloor)
        {
            currentFloor += step;
            var remaining = Math.Abs(toFloor - currentFloor);
            
            if (remaining > 3)
            {
                Console.WriteLine($"   Passing floor {currentFloor}");
            }
            else if (remaining > 0)
            {
                Console.WriteLine($"   Approaching floor {currentFloor} [SLOWING DOWN]");
            }
            else
            {
                Console.WriteLine($"   Arriving at floor {currentFloor}!");
            }
            
            await Task.Delay(baseSpeed);
        }
        
        Console.WriteLine($"   Elevator {elevator.Id} arrived at floor {toFloor}");
        await Task.Delay(300);
    }

    // Synchronous wrapper for testing compatibility
    public void ProcessRequest(List<Elevator> elevators, Request request)
    {
        ProcessRequestAsync(elevators, request).GetAwaiter().GetResult();
    }
}