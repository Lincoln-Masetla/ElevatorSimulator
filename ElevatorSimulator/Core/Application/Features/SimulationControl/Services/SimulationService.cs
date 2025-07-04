using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using ElevatorSimulator.Core.Application.Common.Observers;

namespace ElevatorSimulator.Core.Application.Features.SimulationControl.Services;

/// <summary>
/// Simulation service - Single Responsibility: Coordinating elevator simulation logic
/// </summary>
public class SimulationService : ISimulationService
{
    private readonly IElevatorRepository _elevatorRepository;
    private readonly IElevatorService _elevatorService;
    private readonly IDispatchService _dispatchService;
    private readonly IElevatorObserverService _observerService;
    private readonly IQueueService _queueService;
    private readonly ElevatorLogger _logger;

    public SimulationService(
        IElevatorRepository elevatorRepository,
        IElevatorService elevatorService, 
        IDispatchService dispatchService,
        IElevatorObserverService observerService,
        IQueueService queueService)
    {
        _elevatorRepository = elevatorRepository ?? throw new ArgumentNullException(nameof(elevatorRepository));
        _elevatorService = elevatorService ?? throw new ArgumentNullException(nameof(elevatorService));
        _dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
        _observerService = observerService ?? throw new ArgumentNullException(nameof(observerService));
        _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
        _logger = new ElevatorLogger();

        // Subscribe observers to all elevators
        foreach (var elevator in _elevatorRepository.GetAll())
        {
            _observerService.Subscribe(elevator.Id, _logger);
        }
    }

    public List<Elevator> GetElevators() => _elevatorRepository.GetAll();

    public ElevatorLogger GetLogger() => _logger;

    public async Task<int> ProcessRequestAsync(Request request)
    {
        // Check if any elevator is available
        if (!HasAvailableElevator())
        {
            // Queue the request if no elevators are available
            _queueService.EnqueueRequest(request);
            return request.PassengerCount; // All passengers queued
        }
        
        var remainingPassengers = await _dispatchService.ProcessRequestAsync(_elevatorRepository.GetAll(), request);
        
        // Queue remaining passengers if any
        if (remainingPassengers > 0)
        {
            var queuedRequest = new Request(request.FromFloor, request.ToFloor, remainingPassengers);
            _queueService.EnqueueRequest(queuedRequest);
        }
        
        // After processing, check if there are queued requests
        await ProcessQueuedRequestsAsync();
        
        return remainingPassengers;
    }

    public Elevator? GetClosestElevator(int targetFloor)
    {
        return _dispatchService.FindClosestElevator(_elevatorRepository.GetAll(), targetFloor);
    }

    public async Task UpdateElevatorsAsync()
    {
        var elevators = _elevatorRepository.GetAll();
        var tasks = elevators.Select(async elevator =>
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
    
    public RequestQueue GetRequestQueue() => _queueService.GetQueue();
    
    public bool HasAvailableElevator()
    {
        return _elevatorRepository.GetAvailable().Any();
    }
    
    /// <summary>
    /// Process multiple requests concurrently when possible
    /// </summary>
    public async Task<List<int>> ProcessMultipleRequestsConcurrentlyAsync(List<Request> requests)
    {
        Console.WriteLine($"\nCONCURRENT REQUEST PROCESSING");
        Console.WriteLine($"═══════════════════════════════════");
        Console.WriteLine($"Processing {requests.Count} requests concurrently...");
        
        var tasks = requests.Select(async (request, index) =>
        {
            Console.WriteLine($"\nStarting concurrent processing of Request #{index + 1}:");
            Console.WriteLine($"   {request.PassengerCount} passengers from floor {request.FromFloor} to floor {request.ToFloor}");
            
            return await ProcessRequestAsync(request);
        }).ToArray();
        
        var results = await Task.WhenAll(tasks);
        
        Console.WriteLine($"\nConcurrent processing completed!");
        Console.WriteLine($"   Processed {requests.Count} requests simultaneously");
        
        return results.ToList();
    }

    public async Task ProcessQueuedRequestsAsync()
    {
        Console.WriteLine("\nPROCESSING QUEUED REQUESTS");
        Console.WriteLine("═══════════════════════════════");
        
        int processedCount = 0;
        while (_queueService.HasPendingRequests && HasAvailableElevator())
        {
            var nextRequest = _queueService.DequeueRequest();
            if (nextRequest != null)
            {
                processedCount++;
                Console.WriteLine($"\nProcessing queued request #{processedCount}:");
                Console.WriteLine($"   {nextRequest.PassengerCount} passengers from floor {nextRequest.FromFloor} to floor {nextRequest.ToFloor}");
                
                var remainingPassengers = await _dispatchService.ProcessRequestAsync(_elevatorRepository.GetAll(), nextRequest);
                
                // If there are still remaining passengers, put them back in queue
                if (remainingPassengers > 0)
                {
                    var requeuedRequest = new Request(nextRequest.FromFloor, nextRequest.ToFloor, remainingPassengers);
                    _queueService.EnqueueRequest(requeuedRequest);
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
        
        if (_queueService.HasPendingRequests)
        {
            Console.WriteLine($"{_queueService.PendingCount} request(s) still in queue (waiting for elevator availability)");
        }
        else if (processedCount > 0)
        {
            Console.WriteLine("All queued requests have been processed!");
        }
    }
}