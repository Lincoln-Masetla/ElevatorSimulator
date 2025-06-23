using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Interfaces;
using ElevatorSimulator.Application.Observers;

namespace ElevatorSimulator.Application.Services;

/// <summary>
/// Simulation service implementing clean architecture and Observer pattern
/// </summary>
public class SimulationService : ISimulationService
{
    private readonly List<Elevator> _elevators;
    private readonly IElevatorService _elevatorService;
    private readonly IDispatchService _dispatchService;
    private readonly IElevatorObserverService _observerService;
    private readonly ElevatorLogger _logger;
    private readonly RequestQueue _requestQueue;

    public SimulationService(
        IElevatorService elevatorService, 
        IDispatchService dispatchService,
        IElevatorObserverService observerService)
    {
        _elevatorService = elevatorService;
        _dispatchService = dispatchService;
        _observerService = observerService;
        _logger = new ElevatorLogger();
        _requestQueue = new RequestQueue();
        
        _elevators = new List<Elevator>
        {
            new() { Id = 1, Type = ElevatorType.Standard, MaxCapacity = 8 },
            new() { Id = 2, Type = ElevatorType.Standard, MaxCapacity = 8 },
            new() { Id = 3, Type = ElevatorType.HighSpeed, MaxCapacity = 12 },
            new() { Id = 4, Type = ElevatorType.Freight, MaxCapacity = 20 }
        };

        // Subscribe observers to all elevators
        foreach (var elevator in _elevators)
        {
            _observerService.Subscribe(elevator.Id, _logger);
        }
    }

    public List<Elevator> GetElevators() => _elevators;

    public ElevatorLogger GetLogger() => _logger;

    public async Task<int> ProcessRequestAsync(Request request)
    {
        // Check if any elevator is available
        if (!HasAvailableElevator())
        {
            // Queue the request if no elevators are available
            _requestQueue.Enqueue(request);
            return request.PassengerCount; // All passengers queued
        }
        
        var remainingPassengers = await _dispatchService.ProcessRequestAsync(_elevators, request);
        
        // Queue remaining passengers if any
        if (remainingPassengers > 0)
        {
            var queuedRequest = new Request(request.FromFloor, request.ToFloor, remainingPassengers);
            _requestQueue.Enqueue(queuedRequest);
        }
        
        // After processing, check if there are queued requests
        await ProcessQueuedRequestsAsync();
        
        return remainingPassengers;
    }

    public Elevator? GetClosestElevator(int targetFloor)
    {
        return _dispatchService.FindClosestElevator(_elevators, targetFloor);
    }

    public async Task UpdateElevatorsAsync()
    {
        var tasks = _elevators.Select(async elevator =>
        {
            var nextFloor = _elevatorService.GetNextDestination(elevator);
            if (nextFloor.HasValue)
            {
                await _elevatorService.MoveToFloorAsync(elevator, nextFloor.Value);
                
                // Unload passengers when reaching destination
                if (elevator.State == ElevatorState.DoorsOpen && elevator.PassengerCount > 0)
                {
                    await _elevatorService.UnloadPassengersAsync(elevator, elevator.PassengerCount);
                }
            }
        });

        await Task.WhenAll(tasks);
    }

    public void UpdateElevators()
    {
        // Synchronous wrapper for backward compatibility
        UpdateElevatorsAsync().GetAwaiter().GetResult();
    }
    
    public RequestQueue GetRequestQueue() => _requestQueue;
    
    public bool HasAvailableElevator()
    {
        return _elevators.Any(e => e.State == ElevatorState.Idle && e.PassengerCount < e.MaxCapacity);
    }
    
    public async Task ProcessQueuedRequestsAsync()
    {
        Console.WriteLine("\nPROCESSING QUEUED REQUESTS");
        Console.WriteLine("═══════════════════════════════");
        
        int processedCount = 0;
        while (_requestQueue.HasPendingRequests && HasAvailableElevator())
        {
            var nextRequest = _requestQueue.Dequeue();
            if (nextRequest != null)
            {
                processedCount++;
                Console.WriteLine($"\nProcessing queued request #{processedCount}:");
                Console.WriteLine($"   {nextRequest.PassengerCount} passengers from floor {nextRequest.FromFloor} to floor {nextRequest.ToFloor}");
                
                var remainingPassengers = await _dispatchService.ProcessRequestAsync(_elevators, nextRequest);
                
                // If there are still remaining passengers, put them back in queue
                if (remainingPassengers > 0)
                {
                    var requeuedRequest = new Request(nextRequest.FromFloor, nextRequest.ToFloor, remainingPassengers);
                    _requestQueue.Enqueue(requeuedRequest);
                    Console.WriteLine($"   {remainingPassengers} passengers re-queued due to capacity limits");
                    break; // Stop processing to avoid infinite loop
                }
                
                Console.WriteLine($"   Queued request #{processedCount} completed successfully");
                
                // Small delay for realism
                await Task.Delay(500);
            }
        }
        
        if (processedCount > 0)
        {
            Console.WriteLine($"\nProcessed {processedCount} queued request(s)");
        }
        
        if (_requestQueue.HasPendingRequests)
        {
            Console.WriteLine($"{_requestQueue.PendingCount} request(s) still in queue (waiting for elevator availability)");
        }
        else if (processedCount > 0)
        {
            Console.WriteLine("All queued requests have been processed!");
        }
    }
}