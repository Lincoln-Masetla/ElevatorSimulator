using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

/// <summary>
/// Interface for simulation management following DIP
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Gets all elevators in the system
    /// </summary>
    List<Elevator> GetElevators();
    
    /// <summary>
    /// Processes an elevator request with realistic simulation
    /// Returns the number of passengers that couldn't be accommodated
    /// </summary>
    Task<int> ProcessRequestAsync(Request request);
    
    /// <summary>
    /// Gets closest available elevator to target floor
    /// </summary>
    Elevator? GetClosestElevator(int targetFloor);
    
    /// <summary>
    /// Updates elevator states
    /// </summary>
    void UpdateElevators();
    
    /// <summary>
    /// Gets the request queue for monitoring pending requests
    /// </summary>
    RequestQueue GetRequestQueue();
    
    /// <summary>
    /// Checks if any elevator is available
    /// </summary>
    bool HasAvailableElevator();
    
    /// <summary>
    /// Processes queued requests when elevators become available
    /// </summary>
    Task ProcessQueuedRequestsAsync();
    
    /// <summary>
    /// Process multiple requests concurrently when possible
    /// Returns a list of remaining passengers for each request
    /// </summary>
    Task<List<int>> ProcessMultipleRequestsConcurrentlyAsync(List<Request> requests);
}